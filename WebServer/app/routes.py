from flask import (
    Blueprint,
    jsonify,
    redirect,
    render_template,
    request,
    send_from_directory,
    send_file,
    url_for,
)
from services import FileService
from storage import FileStorageService
from config import Config
import requests
import hashlib
import os
from datetime import datetime, timedelta
from io import BytesIO


def create_routes(
    file_service: FileService, storage: FileStorageService, config: Config, mongo
):
    bp = Blueprint("files", __name__)

    @bp.route("/")
    def index():
        """Zobrazí hlavní stránku s formulárem a seznamem souborů"""
        files = file_service.get_all_files()
        return render_template("index.html", data=files)

    @bp.route("/files", methods=["GET"])
    def get_file_metadata():
        """API endpoint pro získání metadat všech souborů (pro externí aplikaci)"""
        metadata = file_service.get_all_files_metadata()
        return jsonify(metadata)

    @bp.route("/files", methods=["POST"])
    def upload_file():
        """Nahraje nový soubor"""

        if "glbFile" not in request.files or "imgFile" not in request.files:
            return redirect(request.url)

        glb_file = request.files["glbFile"]
        img_file = request.files["imgFile"]

        if glb_file.filename == "" or img_file.filename == "":
            return redirect(request.url)

        lat = request.form.get("lat")
        lon = request.form.get("lon")
        name = request.form.get("name")
        description = request.form.get("description")

        success, message = file_service.upload_files(
            glb_file,
            img_file,
            lat,
            lon,
            name,
            description,
            config.ALLOWED_GLB_EXTENSIONS,
            config.ALLOWED_IMAGE_EXTENSIONS,
        )

        if not success:
            return message, 400

        return redirect(url_for("files.index"))

    @bp.route("/files/<filename>/download")
    def uploaded_file(filename):
        """Stáhne soubor"""
        return send_from_directory(config.UPLOAD_PATH, filename)

    @bp.route("/files/<file_id>", methods=["GET"])
    def get_file_by_id(file_id):
        """Získá detail souboru podle ID"""
        file_data = file_service.get_file_by_id(file_id)
        if not file_data:
            return jsonify({"error": "File not found"}), 404

        return jsonify(
            {
                "_id": file_data._id,
                "glb_filename": file_data.glb_filename,
                "img_filename": file_data.img_filename,
                "lat": file_data.lat,
                "lon": file_data.lon,
                "name": file_data.name,
                "description": file_data.description,
            }
        )

    @bp.route("/files/<file_id>", methods=["PUT", "POST"])
    def update_file_api(file_id):
        """Aktualizuje existující soubor (pro AJAX volání z UI)"""

        lat = request.form.get("lat")
        lon = request.form.get("lon")
        name = request.form.get("name")
        description = request.form.get("description")

        glb_file = request.files.get("glbFile") if "glbFile" in request.files else None
        img_file = request.files.get("imgFile") if "imgFile" in request.files else None

        success, message = file_service.update_file(
            file_id,
            glb_file,
            img_file,
            lat,
            lon,
            name,
            description,
            config.ALLOWED_GLB_EXTENSIONS,
            config.ALLOWED_IMAGE_EXTENSIONS,
        )

        if not success:
            return jsonify({"error": message}), 400

        return jsonify({"message": message, "success": True}), 200

    @bp.route("/files/<file_id>", methods=["DELETE"])
    def delete_file_api(file_id):
        """Smaže soubor (pro AJAX volání z UI)"""
        success, message = file_service.delete_file(file_id)

        if not success:
            return jsonify({"error": message}), 404

        return jsonify({"message": message, "success": True}), 200

    @bp.route("/maps/staticmap")
    def get_cached_map():
        """
        Google Maps Static API s cachováním v MongoDB

        Parametry:
        - lat: latitude
        - lon: longitude
        - zoom: zoom level (default 16)
        - size: rozměry mapy (default 1000x1000)
        - maptype: typ mapy (default roadmap)

        Příklad: /maps/staticmap?lat=50.6602&lon=14.04113&zoom=16&size=1000x1000
        """
        lat = request.args.get("lat")
        lon = request.args.get("lon")
        zoom = request.args.get("zoom", "16")
        size = request.args.get("size", "1000x1000")
        maptype = request.args.get("maptype", "roadmap")

        if not lat or not lon:
            return jsonify({"error": "Missing lat or lon parameter"}), 400

        # Vytvoř unikátní ID z parametrů
        cache_id = hashlib.md5(
            f"{lat}_{lon}_{zoom}_{size}_{maptype}".encode()
        ).hexdigest()

        # Zkus z MongoDB cache
        cached_map = mongo.db.maps_cache.find_one({"_id": cache_id})
        if cached_map and cached_map["expires"] > datetime.now():
            return send_file(BytesIO(cached_map["image"]), mimetype="image/png")

        # Zavolej Google Static Maps API
        google_url = "https://maps.googleapis.com/maps/api/staticmap"
        params = {
            "center": f"{lat},{lon}",
            "zoom": zoom,
            "size": size,
            "scale": "1",
            "maptype": maptype,
            "style": "feature:poi|element:labels|visibility:off",
            "key": os.getenv("GOOGLE_MAPS_API_KEY"),
        }

        print(f"Fetching map from Google with params: {params}")
        print(f"Cache ID: {cache_id}")

        try:
            response = requests.get(google_url, params=params, timeout=10)
            print(f"Google API Response: {response.status_code}")
            response.raise_for_status()
            image_data = response.content
            print(f"Image data received: {len(image_data)} bytes")

        except requests.exceptions.RequestException as e:
            print(f"ERROR fetching from Google Maps: {str(e)}")
            return jsonify({"error": f"Google Maps API error: {str(e)}"}), 500

        # Ulož do cache (24 hodin)
        mongo.db.maps_cache.update_one(
            {"_id": cache_id},
            {
                "$set": {
                    "image": image_data,
                    "expires": datetime.now() + timedelta(days=1),
                    "lat": lat,
                    "lon": lon,
                    "zoom": zoom,
                    "size": size,
                    "maptype": maptype,
                    "created": datetime.now(),
                }
            },
            upsert=True,
        )

        return send_file(BytesIO(image_data), mimetype="image/png")

    return bp
