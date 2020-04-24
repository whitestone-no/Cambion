import time

project = 'Cambion'
copyright = str(time.localtime().tm_year) + ', Whitestone.'
author = 'Whitestone'

source_suffix = ['.rst']
master_doc = 'index'

highlight_language = 'csharp'

html_favicon = 'favicon.ico'
html_logo = 'images/logoTiny.png'
html_theme_options = {
    'logo_only': True,
    'collapse_navigation': False,
    'prev_next_buttons_location': 'None'
}