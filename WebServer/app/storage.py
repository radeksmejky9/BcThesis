import uuid
import os
from werkzeug.datastructures import FileStorage


class FileStorageService:
    def __init__(self, upload_path: str):
        self.upload_path = upload_path
        os.makedirs(self.upload_path, exist_ok=True)

    def save_file(self, file: FileStorage, extension: str) -> str:
        """Uloží soubor a vrátí jeho jedinečné jméno"""
        filename = f"{uuid.uuid4()}.{extension}"
        file_path = os.path.join(self.upload_path, filename)
        file.save(file_path)
        return filename

    def get_file_path(self, filename: str) -> str:
        """Vrátí absolutní cestu k souboru"""
        return os.path.join(self.upload_path, filename)

    def delete_file(self, filename: str) -> bool:
        """Smaže soubor ze systému"""
        try:
            file_path = self.get_file_path(filename)
            if os.path.exists(file_path):
                os.remove(file_path)
                return True
            return False
        except Exception:
            return False
