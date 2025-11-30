FROM python:3.13-slim

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Zkopíruj všechno z app/ do /app/ v containeru
COPY app /app

# Vytvoř složku pro uploady
RUN mkdir -p /app/files

ENV PYTHONUNBUFFERED=1

EXPOSE 5000

WORKDIR /app
CMD ["python", "run.py"]