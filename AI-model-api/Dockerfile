FROM python:3.11-slim

# Install system dependencies for MSSQL ODBC
RUN apt-get update && apt-get install -y \
    gcc g++ gnupg curl unixodbc-dev \
    && curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - \
    && curl https://packages.microsoft.com/config/debian/10/prod.list > /etc/apt/sources.list.d/mssql-release.list \
    && apt-get update && ACCEPT_EULA=Y apt-get install -y msodbcsql17 \
    && rm -rf /var/lib/apt/lists/*

# Declare persistent model storage
VOLUME /app/data

# Set working directory
WORKDIR /app

# Copy app code
COPY . .

# Install Python dependencies
RUN pip install --no-cache-dir -r requirements.txt

# Entry point: train and serve API
CMD ["sh", "-c", "uvicorn AI_model_api:app --host 0.0.0.0 --port 8000"] # without training
#CMD ["sh", "-c", "python -c 'from AI_model_api import train; train()' && uvicorn AI_model_api:app --host 0.0.0.0 --port 8000"] # with training
