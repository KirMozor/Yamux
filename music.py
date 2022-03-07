from yandex_music import Best, Client, Search
import toml
config = toml.load("config.toml")

client = Client(config.get('tokenYandex')).init()

def extractDirectLinkToTrack(track_id):
    track = client.tracks(track_id)[0]
    track_download_info = track.get_download_info()

    is_track_suitable = lambda info: all([
        info.codec == "mp3",
        info.bitrate_in_kbps == 192
    ])

    for info in track_download_info:
        if is_track_suitable(info):
            return info.get_direct_link()
            
def durationTrack(url):
    trackID = url.split('/')[-1]
    track = client.tracks([trackID])[0]
    return track.duration_ms / 1000

def download(url, path):
    trackID = url.split('/')[-1]
    track = client.tracks([trackID])[0]
    trackDownloadInfo = track.get_download_info()[0]
    track = client.tracks([trackID])[0]

    track.download(f'{path}/{track.title}.mp3', 'mp3', 192)
    return f"{path}/{track.title}.mp3"
