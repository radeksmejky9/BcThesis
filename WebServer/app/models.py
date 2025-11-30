from dataclasses import dataclass
from typing import Optional


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
