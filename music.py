from yandex_music import Best, Client, Search
import config as aut

client = Client.from_token(aut.OAUTH)

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