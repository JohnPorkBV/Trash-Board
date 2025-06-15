FROM python:3.11-slim

# System dependencies for MSSQL
RUN apt-get update && apt-get install -y \
    gcc g++ gnupg curl unixodbc-dev \
    && curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - \
    && curl https://packages.microsoft.com/config/debian/10/prod.list > /etc/apt/sources.list.d/mssql-release.list \
    && apt-get update && ACCEPT_EULA=Y apt-get install -y msodbcsql17 \
    && rm -rf /var/lib/apt/lists/*

# Set workdir
WORKDIR /app

# Copy everything
COPY . .

# Install Python deps
RUN pip install --no-cache-dir -r requirements.txt

# Run FastAPI + training (at runtime)
CMD ["sh", "-c", "python AI_model_api_trainer.py && uvicorn main:app --host 0.0.0.0 --port 8000"]
