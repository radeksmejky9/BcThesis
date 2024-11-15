from flask import (
    Flask,
    redirect,
    render_template,
    request,
    send_from_directory,
    url_for,
)
from werkzeug.utils import secure_filename
from flask_pymongo import PyMongo
import os

app = Flask(__name__)

app.config["MONGO_URI"] = "mongodb://mongo:27017/mydatabase"
app.config["UPLOAD_FOLDER"] = "/app/uploads"
app.config["ALLOWED_EXTENSIONS"] = {"obj"}

mongo = PyMongo(app)
os.makedirs(app.config["UPLOAD_FOLDER"], exist_ok=True)
files_collection = mongo.db.files


def allowed_file(filename):
    return (
        "." in filename
        and filename.rsplit(".", 1)[1].lower() in app.config["ALLOWED_EXTENSIONS"]
    )


@app.route("/")
def index():
    return render_template("index.html")


@app.route("/upload", methods=["POST"])
def upload_file():
    if "file" not in request.files:
        print("No file part")
        return redirect(request.url)

    file = request.files["file"]
    if file.filename == "":
        print("No selected file")
        return redirect(request.url)

    lat = request.form.get("lat")
    lon = request.form.get("lon")
    name = request.form.get("name")

    if file and allowed_file(file.filename):
        filename = secure_filename(file.filename)
        file_path = os.path.join(app.config["UPLOAD_FOLDER"], filename)
        print(f"Saving file to {file_path}")
        file.save(file_path)

        file_url = url_for("uploaded_file", filename=filename, _external=True)

        file_metadata = {
            "filename": filename,
            "file_path": file_path,
            "file_url": file_url,
            "lat": lat,
            "lon": lon,
            "name": name,
        }
        files_collection.insert_one(file_metadata)

        print("File uploaded and metadata saved.")
        return render_template("index.html", filename=filename)

    print("File type not allowed")
    return "File type not allowed", 400


@app.route("/uploads/<filename>")
def uploaded_file(filename):
    return send_from_directory(app.config["UPLOAD_FOLDER"], filename)


@app.route("/files/")
def files():
    return send_from_directory(app.config["UPLOAD_FOLDER"], "couch.obj")


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000, debug=True)
