#python api_toxicidad.py Esto para ejecutar la ia

from fastapi import FastAPI
from pydantic import BaseModel
from transformers import pipeline

app = FastAPI(title="API de Toxicidad Local")
print("Cargando modelo de Hugging Face... (esto puede tardar unos segundos)")
clasificador = pipeline(
    "text-classification",
    model="textdetox/bert-multilingual-toxicity-classifier"
)
print("¡Modelo cargado y listo!")

class TextoRequest(BaseModel):
    texto: str

@app.post("/clasificar")
def clasificar_comentario(request: TextoRequest):
    resultado = clasificador(request.texto)[0]
    
    label = resultado["label"]
    score = resultado["score"]
    
    toxicidad = int(round(score * 100)) if label == "LABEL_1" else int(round((1 - score) * 100))
    
    if label == "LABEL_1":
        nivel_toxicidad = int(round(score * 100))
    else:
        nivel_toxicidad = int(round((1 - score) * 100))

    return {
        "label": label,
        "toxicidad_porcentaje": nivel_toxicidad,
        "detalle": resultado
    }

if __name__ == "__main__":
    import uvicorn
    # Corre en el puerto 8000
    uvicorn.run(app, host="127.0.0.1", port=8000)