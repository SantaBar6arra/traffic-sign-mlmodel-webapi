# import the necessary packages
from tensorflow.keras.models import load_model
from skimage import transform
from skimage import exposure
from skimage import io
from imutils import paths
import numpy as np
import argparse
import imutils
import random
import cv2
import os

# construct the argument parse and parse the arguments
ap = argparse.ArgumentParser()
ap.add_argument("-m", "--model", required=True,
	help="path to pre-trained traffic sign recognizer")
ap.add_argument("-i", "--images", required=True,
	help="path to testing directory containing images")
ap.add_argument("-o", "--output", required=True,
	help="path to output examples directory")
args = vars(ap.parse_args())

# load the traffic sign recognizer model
model = load_model(args["model"])

# load the label names
labelNames = open("signnames.csv").read().strip().split("\n")[1:]
labelNames = [l.split(",")[1] for l in labelNames]

# grab the paths to the input images
image_paths = list(paths.list_images(args["images"]))

# open file for results and clear all its contents
file_path = os.path.sep.join([args["output"], "prediction_results.txt"])
file = open(file_path, "w").close()

results = []

# loop over image paths
for (i, image_path) in enumerate(image_paths):
	image = io.imread(image_path)
	image = transform.resize(image, (32, 32))
	image = exposure.equalize_adapthist(image, clip_limit=0.1)

	# preprocess the image by scaling it to the range [0, 1]
	image = image.astype("float32") / 255.0
	image = np.expand_dims(image, axis=0)

	# make predictions using the traffic sign recognizer CNN
	preds = model.predict(image)
	j = preds.argmax(axis=1)[0]
	label = labelNames[j]

	prediction_probabilities = []
	for i in range(0, len(labelNames)):
		prediction_probabilities.append((labelNames[i], preds[0][i]))
	prediction_probabilities = sorted(prediction_probabilities, key = lambda x: x[1], reverse=True)[0:5]

	results.append(prediction_probabilities)

print('\t')
for i in range (0, len(results)):
	print(f'Image {i}')
	for a, b in results[i]:
		print(a + ': ' + str(b) + ';')
	print()
