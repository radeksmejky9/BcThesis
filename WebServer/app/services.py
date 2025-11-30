from werkzeug.datastructures import FileStorage
from typing import List, Tuple, Optional
from models import FileMetadata
from repositories import FileRepository
from storage import FileStorageService


class ValidationService:
    @staticmethod
    def validate_file_extension(filename: str, allowed_extensions: set) -> bool:
        """Validuje příponu souboru"""
        if not filename:
            return False
        return (
            "." in filename and filename.rsplit(".", 1)[1].lower() in allowed_extensions
        )

    @staticmethod
    def get_file_extension(filename: str) -> str:
        """Vrátí příponu souboru"""
        return filename.rsplit(".", 1)[1].lower()

    @staticmethod
    def validate_coordinates(lat: str, lon: str) -> bool:
        """Validuje GPS souřadnice"""
        try:
            lat_float = float(lat)
            lon_float = float(lon)
            return -90 <= lat_float <= 90 and -180 <= lon_float <= 180
        except (ValueError, TypeError):
            return False


class FileService:
    def __init__(
        self,
        repository: FileRepository,
        storage: FileStorageService,
        validation: ValidationService,
    ):
        self.repository = repository
        self.storage = storage
        self.validation = validation

    def get_all_files(self) -> List[FileMetadata]:
        """Vrátí všechny soubory jako objekty FileMetadata"""
        files = self.repository.find_all()
        return [FileMetadata.from_dict(f) for f in files]

    def get_all_files_metadata(self) -> List[dict]:
        """Vrátí metadata všech souborů pro API"""
        files = self.repository.find_all()
        for file in files:
            file["_id"] = str(file["_id"])
        return files

    def get_file_by_id(self, file_id: str) -> Optional[FileMetadata]:
        """Vrátí soubor podle ID"""
        file_data = self.repository.find_by_id(file_id)
        if file_data:
            return FileMetadata.from_dict(file_data)
        return None

    def upload_files(
        self,
        glb_file: FileStorage,
        img_file: FileStorage,
        lat: str,
        lon: str,
        name: str,
        description: str,
        allowed_glb_ext: set,
        allowed_img_ext: set,
    ) -> Tuple[bool, str]:
        """
        Nahraje soubory a uloží metadata
        Vrací: (success: bool, message: str)
        """

        if not self.validation.validate_file_extension(
            glb_file.filename, allowed_glb_ext
        ):
            return False, "Invalid GLB file type"

        if not self.validation.validate_file_extension(
            img_file.filename, allowed_img_ext
        ):
            return False, "Invalid image file type"

        if not self.validation.validate_coordinates(lat, lon):
            return False, "Invalid coordinates"

        glb_ext = self.validation.get_file_extension(glb_file.filename)
        img_ext = self.validation.get_file_extension(img_file.filename)

        glb_filename = self.storage.save_file(glb_file, glb_ext)
        img_filename = self.storage.save_file(img_file, img_ext)

        metadata = FileMetadata(
            glb_filename=glb_filename,
            img_filename=img_filename,
            lat=lat,
            lon=lon,
            name=name,
            description=description,
        )

        file_id = self.repository.insert(metadata.to_dict())

        return True, f"Files uploaded successfully with ID: {file_id}"

    def update_file(
        self,
        file_id: str,
        glb_file: Optional[FileStorage],
        img_file: Optional[FileStorage],
        lat: str,
        lon: str,
        name: str,
        description: str,
        allowed_glb_ext: set,
        allowed_img_ext: set,
    ) -> Tuple[bool, str]:
        """
        Aktualizuje existující soubor
        Vrací: (success: bool, message: str)
        """

        existing = self.repository.find_by_id(file_id)
        if not existing:
            return False, "File not found"

        if not self.validation.validate_coordinates(lat, lon):
            return False, "Invalid coordinates"

        glb_filename = existing.get("glb_filename")
        img_filename = existing.get("img_filename")

        if glb_file is not None:
            if not self.validation.validate_file_extension(
                glb_file.filename, allowed_glb_ext
            ):
                return False, "Invalid GLB file type"

            self.storage.delete_file(glb_filename)

            glb_ext = self.validation.get_file_extension(glb_file.filename)
            glb_filename = self.storage.save_file(glb_file, glb_ext)

        if img_file is not None:
            if not self.validation.validate_file_extension(
                img_file.filename, allowed_img_ext
            ):
                return False, "Invalid image file type"

            self.storage.delete_file(img_filename)

            img_ext = self.validation.get_file_extension(img_file.filename)
            img_filename = self.storage.save_file(img_file, img_ext)

        updated_metadata = {
            "glb_filename": glb_filename,
            "img_filename": img_filename,
            "lat": lat,
            "lon": lon,
            "name": name,
            "description": description,
        }

        success = self.repository.update(file_id, updated_metadata)

        if success:
            return True, "File updated successfully"

        return True, "File updated successfully (no changes)"

    def delete_file(self, file_id: str) -> Tuple[bool, str]:
        """
        Smaže soubor včetně fyzických souborů
        Vrací: (success: bool, message: str)
        """

        file_data = self.repository.find_by_id(file_id)
        if not file_data:
            return False, "File not found"

        self.storage.delete_file(file_data.get("glb_filename"))
        self.storage.delete_file(file_data.get("img_filename"))
        success = self.repository.delete(file_id)

        if success:
            return True, "File deleted successfully"
        return False, "Delete failed"

    def drop_all_files(self) -> None:
        """Smaže všechny záznamy z databáze"""
        self.repository.drop_collection()
