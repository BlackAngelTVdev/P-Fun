#!/bin/bash
# ============================================
# Export du rapport Markdown vers PDF
# Usage : bash export-pdf.sh
# ============================================

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
INPUT="$SCRIPT_DIR/rapport.md"
OUTPUT="$SCRIPT_DIR/rapport.pdf"
CSS="$SCRIPT_DIR/style.css"

# --- Vérifications ---
if [ ! -f "$INPUT" ]; then
  echo "❌ Fichier introuvable : $INPUT"
  exit 1
fi

if [ ! -f "$CSS" ]; then
  echo "⚠️  Pas de fichier style.css, export sans style custom."
  npx --yes md-to-pdf "$INPUT" --pdf-options '{"format":"A4","margin":{"top":"20mm","bottom":"20mm","left":"15mm","right":"15mm"}}'
  # md-to-pdf crée soit rapport.md.pdf soit rapport.pdf
  if [ -f "$INPUT.pdf" ]; then mv "$INPUT.pdf" "$OUTPUT"; fi
  echo "✅ PDF généré : $OUTPUT"
  exit 0
fi

# --- Export avec CSS custom ---
npx --yes md-to-pdf "$INPUT" \
  --stylesheet "$CSS" \
  --pdf-options '{"format":"A4","margin":{"top":"20mm","bottom":"20mm","left":"15mm","right":"15mm"}}'

# --- Vérifier le fichier généré ---
# md-to-pdf peut créer soit rapport.md.pdf soit rapport.pdf
GENERATED=""
if [ -f "$INPUT.pdf" ]; then
  GENERATED="$INPUT.pdf"
elif [ -f "$OUTPUT" ]; then
  GENERATED="$OUTPUT"
fi

if [ -n "$GENERATED" ]; then
  if [ "$GENERATED" != "$OUTPUT" ]; then
    mv "$GENERATED" "$OUTPUT"
  fi
  echo "✅ PDF généré : $OUTPUT"
else
  echo "❌ Erreur lors de la génération du PDF."
  exit 1
fi
