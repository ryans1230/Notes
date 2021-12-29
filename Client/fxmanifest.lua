fx_version 'cerulean'
games { 'gta5' }

files {
	"nui/**/*.js",
	"nui/**/*.html",
	"nui/**/*.css",
	"nui/**/*.png",
	"Newtonsoft.Json.dll"
}

ui_page "nui/index.html"

client_scripts {
	'Notes.Client.net.dll'
}

server_scripts {
	'Notes.Server.net.dll'
}