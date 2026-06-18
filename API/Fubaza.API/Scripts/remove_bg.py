from rembg import remove
from PIL import Image
import sys

input_path = sys.argv[1]
output_path = sys.argv[2]

input_image = Image.open(input_path).convert("RGBA")
output = remove(input_image)
output.save(output_path)
