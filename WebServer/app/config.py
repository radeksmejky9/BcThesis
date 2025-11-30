import os


class Config:
    MONGO_URI = "mongodb://mongo:27017/mydatabase"
    UPLOAD_PATH = "/app/files"
    UPLOAD_FOLDER_URL = "/files/"
    ALLOWED_GLB_EXTENSIONS = {"glb"}
    ALLOWED_IMAGE_EXTENSIONS = {"jpg", "jpeg", "png", "gif"}
