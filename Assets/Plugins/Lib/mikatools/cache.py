from mikatools import pickle_dump, pickle_load

_cache_dict = {}

def clear_cache():
	global _cache_dict
	_cache_dict = {}

class cache(object):
	"""docstring for cache"""
	def __init__(self, file_path, refresh=False, use_ram=False, indentify_by_args=False, indentify_by_kwargs=False):
		self.file_path = file_path
		self.refresh = refresh
		self.use_ram = use_ram
		self.indentify_by_args = indentify_by_args
		self.indentify_by_kwargs = indentify_by_kwargs

	def _resolve_filename(self, args, kwargs):
		file_path = self.file_path
		if self.indentify_by_args:
			file_path += u"_".join(args)
		if self.indentify_by_kwargs:
			keys = kwargs.keys()
			keys.sort()
			for key in keys:
				file_path += "_" + key +"-" + kwargs[key]
		return file_path

	def __call__(self, f):
		def wrapper(*args, **kwargs):
			file_path = _resolve_filename(args, kwargs)
			if self.use_ram and not self.refresh and file_path in _cache_dict:
				return _cache_dict[file_path]
			elif not self.use_ram and not self.refresh and file_exists(file_path):
				return pickle_load(file_path)
			else:
				d = f(*args, **kwargs)
				if use_ram:
					_cache_dict[file_path] = d
				else:
					pickle_dump(d, file_path)
				return d
		return wrapper