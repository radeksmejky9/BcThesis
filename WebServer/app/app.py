from flask import Flask
from flask_pymongo import PyMongo
from config import Config
from dotenv import load_dotenv
from storage import FileStorageService
from repositories import FileRepository
from services import ValidationService, FileService
from routes import create_routes


def create_app(config_class=Config):
    load_dotenv()
    app = Flask(__name__)

    app.config["MONGO_URI"] = config_class.MONGO_URI
    app.config["UPLOAD_PATH"] = config_class.UPLOAD_PATH
    app.config["UPLOAD_FOLDER_URL"] = config_class.UPLOAD_FOLDER_URL

    mongo = PyMongo(app)

    storage_service = FileStorageService(config_class.UPLOAD_PATH)
    repository = FileRepository(mongo)
    validation_service = ValidationService()
    file_service = FileService(repository, storage_service, validation_service)

    routes_bp = create_routes(file_service, storage_service, config_class, mongo)
    app.register_blueprint(routes_bp)

    return app
