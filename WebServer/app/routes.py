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
from services import FileService, RatingService, MapCacheService
from config import Config
from io import BytesIO


def create_routes(
    file_service: FileService,
    rating_service: RatingService,
    map_cache_service: MapCacheService,
    config: Config,
):
    bp = Blueprint("files", __name__)

    @bp.route("/")
    def index():
        """Zobrazí hlavní stránku s formulářem a seznamem souborů"""
        files = file_service.get_all_files()
        files_with_ratings = []
        for file in files:
            file_dict = file.to_dict()
            avg_rating = rating_service.get_average_rating(file._id)
            file_dict["rating"] = avg_rating
            files_with_ratings.append(file_dict)

        return render_template("index.html", data=files_with_ratings)

    @bp.route("/files", methods=["GET"])
    def get_file_metadata():
        """API endpoint pro získání metadat všech souborů"""
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
        if filename.endswith(".glb"):
            mimetype = "model/gltf-binary"
        elif filename.endswith((".jpg", ".jpeg", ".png", ".gif")):
            mimetype = "image/jpeg"
        else:
            mimetype = "application/octet-stream"

        return send_from_directory(config.UPLOAD_PATH, filename, mimetype=mimetype)

    @bp.route("/files/<file_id>", methods=["GET"])
    def get_file_by_id(file_id):
        """Získá detail souboru podle ID"""
        file_data = file_service.get_file_by_id(file_id)
        if not file_data:
            return jsonify({"error": "File not found"}), 404
        ratings = rating_service.get_ratings_for_file(file_id)
        avg_rating = rating_service.get_average_rating(file_id)

        return jsonify(
            {
                "_id": file_data._id,
                "glb_filename": file_data.glb_filename,
                "img_filename": file_data.img_filename,
                "lat": file_data.lat,
                "lon": file_data.lon,
                "name": file_data.name,
                "description": file_data.description,
                "ratings": [r.to_dict() for r in ratings],
                "average_rating": avg_rating,
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
        # Smaž také všechna hodnocení
        rating_service.delete_ratings_for_file(file_id)

        success, message = file_service.delete_file(file_id)

        if not success:
            return jsonify({"error": message}), 404

        return jsonify({"message": message, "success": True}), 200

    @bp.route("/files/<file_id>/ratings", methods=["GET"])
    def get_ratings(file_id):
        """Získá všechna hodnocení pro soubor"""
        ratings = rating_service.get_ratings_for_file(file_id)
        avg_rating = rating_service.get_average_rating(file_id)

        return jsonify(
            {
                "ratings": [
                    {
                        "_id": r._id,
                        "stars": r.stars,
                        "comment": r.comment,
                        "created_at": (
                            r.created_at.isoformat() if r.created_at else None
                        ),
                    }
                    for r in ratings
                ],
                "average_rating": avg_rating,
            }
        )

    @bp.route("/files/<file_id>/ratings", methods=["POST"])
    def add_rating(file_id):
        """Přidá nové hodnocení k souboru"""
        file_data = file_service.get_file_by_id(file_id)
        if not file_data:
            return jsonify({"error": "File not found"}), 404

        data = request.get_json()
        if not data:
            return jsonify({"error": "No data provided"}), 400

        stars = data.get("stars")
        comment = data.get("comment", "")
        print(comment)
        try:
            stars = int(stars)
        except (ValueError, TypeError):
            return jsonify({"error": "Stars must be a number"}), 400

        success, message = rating_service.add_rating(file_id, stars, comment)

        if not success:
            return jsonify({"error": message}), 400

        return jsonify({"message": message, "success": True}), 201

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

        success, image_data, error_message = map_cache_service.get_cached_map(
            lat, lon, zoom, size, maptype
        )

        if not success:
            return jsonify({"error": error_message}), 500

        return send_file(BytesIO(image_data), mimetype="image/png")

    return bp
