import time

source_suffix = ['.rst']

master_doc = 'index'

project = 'Cambion'
copyright = str(time.localtime().tm_year) + ', Whitestone.'
author = 'Whitestone'

highlight_language = 'csharp'

html_theme_options = {
	'collapse_navigation': False,
	'prev_next_buttons_location': None
}