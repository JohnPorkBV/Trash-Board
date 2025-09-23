FROM python:3.11-slim

RUN apt-get update && apt-get install -y \
    gcc g++ gnupg curl unixodbc-dev \
    && curl -sSL https://packages.microsoft.com/keys/microsoft.asc \
       | gpg --dearmor \
       | tee /usr/share/keyrings/microsoft-prod.gpg > /dev/null \
    && echo "deb [arch=amd64,arm64,armhf signed-by=/usr/share/keyrings/microsoft-prod.gpg] https://packages.microsoft.com/debian/12/prod bookworm main" \
       | tee /etc/apt/sources.list.d/mssql-release.list \
    && apt-get update \
    && ACCEPT_EULA=Y apt-get install -y msodbcsql17 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY . .
RUN pip install --no-cache-dir -r requirements.txt

CMD ["uvicorn", "AI_model_api:app", "--host", "0.0.0.0", "--port", "8000"]
