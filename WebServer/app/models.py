from dataclasses import dataclass
from typing import Optional
from datetime import datetime, timedelta
import hashlib


@dataclass
class FileMetadata:
    glb_filename: str
    img_filename: str
    lat: str
    lon: str
    name: str
    description: str
    _id: Optional[str] = None

    def to_dict(self):
        data = {
            "glb_filename": self.glb_filename,
            "img_filename": self.img_filename,
            "lat": self.lat,
            "lon": self.lon,
            "name": self.name,
            "description": self.description,
        }
        if self._id:
            data["_id"] = self._id
        return data

    @classmethod
    def from_dict(cls, data: dict):
        return cls(
            glb_filename=data.get("glb_filename"),
            img_filename=data.get("img_filename"),
            lat=data.get("lat"),
            lon=data.get("lon"),
            name=data.get("name"),
            description=data.get("description"),
            _id=str(data.get("_id")) if "_id" in data else None,
        )


@dataclass
class Rating:
    file_id: str
    stars: int
    comment: str
    created_at: Optional[datetime] = None
    _id: Optional[str] = None

    def to_dict(self):
        data = {
            "file_id": self.file_id,
            "stars": self.stars,
            "comment": self.comment,
            "created_at": self.created_at or datetime.now(),
        }
        if self._id:
            data["_id"] = self._id
        return data

    @classmethod
    def from_dict(cls, data: dict):
        return cls(
            file_id=str(data.get("file_id")),
            stars=data.get("stars"),
            comment=data.get("comment"),
            created_at=data.get("created_at"),
            _id=str(data.get("_id")) if "_id" in data else None,
        )


@dataclass
class MapCache:
    lat: str
    lon: str
    zoom: str
    size: str
    maptype: str
    image: bytes
    expires: Optional[datetime] = None
    created: Optional[datetime] = None
    _id: Optional[str] = None

    def __post_init__(self):
        """Automaticky nastaví expires a created pokud nejsou zadány"""
        if self.created is None:
            self.created = datetime.now()
        if self.expires is None:
            self.expires = datetime.now() + timedelta(days=1)
        if self._id is None:
            self._id = self.generate_cache_id()

    def generate_cache_id(self) -> str:
        """Generuje unikátní cache ID na základě parametrů"""
        return hashlib.md5(
            f"{self.lat}_{self.lon}_{self.zoom}_{self.size}_{self.maptype}".encode()
        ).hexdigest()

    def to_dict(self):
        return {
            "_id": self._id,
            "lat": self.lat,
            "lon": self.lon,
            "zoom": self.zoom,
            "size": self.size,
            "maptype": self.maptype,
            "image": self.image,
            "expires": self.expires,
            "created": self.created,
        }

    @classmethod
    def from_dict(cls, data: dict):
        return cls(
            lat=data.get("lat"),
            lon=data.get("lon"),
            zoom=data.get("zoom"),
            size=data.get("size"),
            maptype=data.get("maptype"),
            image=data.get("image"),
            expires=data.get("expires"),
            created=data.get("created"),
            _id=data.get("_id"),
        )
