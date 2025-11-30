from typing import List, Optional
from flask_pymongo import PyMongo
from bson import ObjectId
from datetime import datetime
from models import MapCache


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


class RatingRepository:
    def __init__(self, mongo: PyMongo):
        self.collection = mongo.db.ratings

    def find_by_file_id(self, file_id: str) -> List[dict]:
        """Vrátí všechna hodnocení pro daný soubor"""
        return list(self.collection.find({"file_id": file_id}).sort("created_at", -1))

    def insert(self, rating: dict) -> str:
        """Vloží nové hodnocení"""
        result = self.collection.insert_one(rating)
        return str(result.inserted_id)

    def delete_by_file_id(self, file_id: str) -> bool:
        """Smaže všechna hodnocení pro daný soubor"""
        try:
            self.collection.delete_many({"file_id": file_id})
            return True
        except:
            return False

    def get_average_rating(self, file_id: str) -> Optional[float]:
        """Vypočítá průměrné hodnocení pro soubor"""
        pipeline = [
            {"$match": {"file_id": file_id}},
            {"$group": {"_id": None, "avg": {"$avg": "$stars"}, "count": {"$sum": 1}}},
        ]
        result = list(self.collection.aggregate(pipeline))
        if result:
            return {"average": round(result[0]["avg"], 1), "count": result[0]["count"]}
        return None


class MapCacheRepository:
    def __init__(self, mongo: PyMongo):
        self.collection = mongo.db.maps_cache

    def find_by_params(
        self, lat: str, lon: str, zoom: str, size: str, maptype: str
    ) -> Optional[MapCache]:
        """Najde cachovanou mapu podle parametrů"""
        import hashlib

        cache_id = hashlib.md5(
            f"{lat}_{lon}_{zoom}_{size}_{maptype}".encode()
        ).hexdigest()

        cached = self.collection.find_one({"_id": cache_id})

        if cached and cached.get("expires", datetime.min) > datetime.now():
            return MapCache.from_dict(cached)

        return None

    def upsert(self, map_cache: MapCache) -> str:
        """Vloží nebo aktualizuje cachovanou mapu"""
        result = self.collection.update_one(
            {"_id": map_cache._id}, {"$set": map_cache.to_dict()}, upsert=True
        )
        return map_cache._id

    def delete_expired(self) -> int:
        """Smaže všechny expirované mapy"""
        result = self.collection.delete_many({"expires": {"$lt": datetime.now()}})
        return result.deleted_count

    def drop_collection(self) -> None:
        """Smaže celou kolekci"""
        self.collection.drop()
