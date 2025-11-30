from typing import List, Optional
from flask_pymongo import PyMongo
from bson import ObjectId


class FileRepository:
    def __init__(self, mongo: PyMongo):
        self.collection = mongo.db.files

    def find_all(self) -> List[dict]:
        """Vrátí všechny soubory z databáze"""
        return list(self.collection.find())

    def find_by_id(self, file_id: str) -> Optional[dict]:
        """Najde soubor podle ID"""
        try:
            return self.collection.find_one({"_id": ObjectId(file_id)})
        except:
            return None

    def insert(self, file_metadata: dict) -> str:
        """Vloží nový záznam a vrátí jeho ID"""
        result = self.collection.insert_one(file_metadata)
        return str(result.inserted_id)

    def update(self, file_id: str, file_metadata: dict) -> bool:
        """Aktualizuje existující záznam"""
        try:
            result = self.collection.update_one(
                {"_id": ObjectId(file_id)}, {"$set": file_metadata}
            )
            return result.matched_count > 0
        except Exception as e:
            print(f"Update error: {e}")
            return False

    def delete(self, file_id: str) -> bool:
        """Smaže soubor z databáze"""
        try:
            result = self.collection.delete_one({"_id": ObjectId(file_id)})
            return result.deleted_count > 0
        except:
            return False

    def drop_collection(self) -> None:
        """Smaže celou kolekci"""
        self.collection.drop()
