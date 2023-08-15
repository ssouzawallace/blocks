def make_mt_valid(file1, file2):
	sources = codecs.open(file1, "r", encoding="utf-8").read().split("\n")
	targets = codecs.open(file2, "r", encoding="utf-8").read().split("\n")
	
	source_valid = codecs.open(file1.replace(".txt", "") + "_valid.txt", "w", encoding="utf-8")
	target_valid = codecs.open(file2.replace(".txt", "") + "_valid.txt", "w", encoding="utf-8")
	random.seed()
	if len(sources) > 5000:
		valids = random.sample(range(len(sources)), 5000)
	else:
		valids = random.sample(range(len(sources)), len(sources)/2)
	for valid in valids:
		source_valid.write(sources[valid] + "\n")
		target_valid.write(targets[valid] + "\n")
	source_valid.close()
	target_valid.close()