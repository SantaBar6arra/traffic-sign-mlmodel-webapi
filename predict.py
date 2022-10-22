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
ap.add_argument("-i", "--image", required=True,
	help="path to testing directory containing images")
ap.add_argument("-o", "--output", required=True,
	help="path to output examples directory")
args = vars(ap.parse_args())

# load the traffic sign recognizer model
# print("[INFO] loading model...")
model = load_model(args["model"])

# load the label names
labelNames = open("signnames.csv").read().strip().split("\n")[1:]
labelNames = [l.split(",")[1] for l in labelNames]

# grab the paths to the input images, shuffle them, and grab a sample
# print("[INFO] predicting...")
# imagePaths = list(paths.list_images(args["images"]))
# random.shuffle(imagePaths)
# imagePaths = imagePaths[:25]

image_path = args["image"]

# open file for results and clear all its contents
file_path = os.path.sep.join([args["output"], "prediction_results.txt"])
file = open(file_path, "w").close()

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

# load the image using OpenCV, resize it, and draw the label
# on it
image = cv2.imread(image_path)
image = imutils.resize(image, width=128)
cv2.putText(image, label, (5, 15), cv2.FONT_HERSHEY_SIMPLEX,
	0.45, (0, 0, 255), 2)

# write results of prediction to file
file = open(file_path, "a")
file.write(label + "\n")

print()
print("|" + label)

# save the image to disk
p = os.path.sep.join([args["output"], "{}.png".format(0)])
cv2.imwrite(p, image)

# loop over the image paths
# for (i, imagePath) in enumerate(imagePaths):
# 	# load the image, resize it to 32x32 pixels, and then apply
# 	# Contrast Limited Adaptive Histogram Equalization (CLAHE),
# 	# just like we did during training
# 	image = io.imread(imagePath)
# 	image = transform.resize(image, (32, 32))
# 	image = exposure.equalize_adapthist(image, clip_limit=0.1)
#
# 	# preprocess the image by scaling it to the range [0, 1]
# 	image = image.astype("float32") / 255.0
# 	image = np.expand_dims(image, axis=0)
#
# 	# make predictions using the traffic sign recognizer CNN
# 	preds = model.predict(image)
# 	j = preds.argmax(axis=1)[0]
# 	label = labelNames[j]
#
# 	# load the image using OpenCV, resize it, and draw the label
# 	# on it
# 	image = cv2.imread(imagePath)
# 	image = imutils.resize(image, width=128)
# 	cv2.putText(image, label, (5, 15), cv2.FONT_HERSHEY_SIMPLEX,
# 		0.45, (0, 0, 255), 2)
#
# 	# write results of prediction to file
# 	file = open(file_path, "a")
# 	file.write(str(i) + " : " + label + "\n")
#
# 	# save the image to disk
# 	p = os.path.sep.join([args["examples"], "{}.png".format(i)])
# 	cv2.imwrite(p, image)
