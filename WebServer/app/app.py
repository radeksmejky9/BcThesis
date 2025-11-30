from flask import Flask
from flask_pymongo import PyMongo
from config import Config
from dotenv import load_dotenv
from storage import FileStorageService
from repositories import FileRepository, RatingRepository, MapCacheRepository
from services import ValidationService, FileService, RatingService, MapCacheService
from routes import create_routes


def create_app(config_class=Config):
    load_dotenv()
    app = Flask(__name__)

    app.config["MONGO_URI"] = config_class.MONGO_URI
    app.config["UPLOAD_PATH"] = config_class.UPLOAD_PATH
    app.config["UPLOAD_FOLDER_URL"] = config_class.UPLOAD_FOLDER_URL

    mongo = PyMongo(app)

    # Storage
    storage_service = FileStorageService(config_class.UPLOAD_PATH)

    # Repositories
    file_repository = FileRepository(mongo)
    rating_repository = RatingRepository(mongo)
    map_cache_repository = MapCacheRepository(mongo)

    # Services
    validation_service = ValidationService()
    file_service = FileService(file_repository, storage_service, validation_service)
    rating_service = RatingService(rating_repository, validation_service)
    map_cache_service = MapCacheService(map_cache_repository, validation_service)

    # Routes
    routes_bp = create_routes(
        file_service, rating_service, map_cache_service, storage_service, config_class
    )
    app.register_blueprint(routes_bp)

    return app
