from flask import (
    Flask,
    jsonify,
    redirect,
    render_template,
    request,
    send_from_directory,
    url_for,
)
from werkzeug.utils import secure_filename
from flask_pymongo import PyMongo
import os
import uuid

app = Flask(__name__)

app.config["MONGO_URI"] = "mongodb://mongo:27017/mydatabase"
app.config["UPLOAD_PATH"] = "/app/files"
app.config["UPLOAD_FOLDER_URL"] = "/files/"

mongo = PyMongo(app)
os.makedirs(app.config["UPLOAD_PATH"], exist_ok=True)
files_collection = mongo.db.files


def allowed_file(filename, extensions):
    return "." in filename and filename.rsplit(".", 1)[1].lower() in extensions


@app.route("/")
def index():
    data = list(files_collection.find())
    return render_template("index.html", data=data)


@app.route("/files", methods=["GET"])
def get_file_metadata():
    metadata = list(files_collection.find())
    for file in metadata:
        file["_id"] = str(file["_id"])
    return jsonify(metadata)


@app.route("/files", methods=["POST"])
def upload_file():
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

    if allowed_file(glb_file.filename, ["glb"]) and allowed_file(
        img_file.filename, ["jpg", "jpeg", "png", "gif"]
    ):
        glb_ext = glb_file.filename.rsplit(".", 1)[1].lower()
        img_ext = img_file.filename.rsplit(".", 1)[1].lower()

        glb_filename = f"{uuid.uuid4()}.{glb_ext}"
        img_filename = f"{uuid.uuid4()}.{img_ext}"

        glb_file_path = os.path.join(app.config["UPLOAD_PATH"], glb_filename)
        img_file_path = os.path.join(app.config["UPLOAD_PATH"], img_filename)

        glb_file.save(glb_file_path)
        img_file.save(img_file_path)

        file_metadata = {
            "glb_filename": glb_filename,
            "img_filename": img_filename,
            "lat": lat,
            "lon": lon,
            "name": name,
            "description": description,
        }
        files_collection.insert_one(file_metadata)
        data = list(files_collection.find())
        return render_template("index.html", data=data)

    return "File type not allowed", 400


@app.route("/files/<filename>/download")
def uploaded_file(filename):
    return send_from_directory(app.config["UPLOAD_PATH"], filename)


@app.route("/dropdb")
def drop_db():
    mongo.db.drop_collection("files")
    return "Database dropped"


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000, debug=True)
