from yandex_music import Best, Client, Search
import yandex_music
import toml
config = toml.load("config.toml")

client = Client(config.get('token_yandex')).init()

def extract_direct_link_to_track(track_id):
    track = client.tracks(track_id)[0]
    track_download_info = track.get_download_info()

    is_track_suitable = lambda info: all([
        info.codec == "mp3",
        info.bitrate_in_kbps == 192
    ])

    for info in track_download_info:
        if is_track_suitable(info):
            return info.get_direct_link()
            
def duration_track(url):
    track_id = url.split('/')[-1]
    track = client.tracks([track_id])[0]
    return track.duration_ms / 1000

def duration_track_id(track_id):
    track = client.tracks([track_id])[0]
    return track.duration_ms / 1000

def my_wave():
    try:
        wave = client.rotor_station_tracks("user:onyourwave")
        for i in wave.sequence:
            artist = i.track.artists[0]
            return {'responce':'ok', 'id':i.track.id, 'title':i.track.title, 'artist':artist.name}
    except yandex_music.exceptions.NetworkError:
        return {'responce':'error', 'text':'NetworkError'}
    except yandex_music.exceptions.TimedOutError:
        return {'responce':'error', 'text':'TimedOutError'}

def download(url, path):
    try:
        track_id = url.split('/')[-1]
        track = client.tracks([trackID])[0]
        track_download_info = track.get_download_info()[0]
        track = client.tracks([trackID])[0]

        track.download(f'{path}/{track.title}.mp3', 'mp3', 192)
        return {'responce':'ok', 'text':f'{path}/{track.title}.mp3'}
    except yandex_music.exceptions.NetworkError:
        return {'responce':'error', 'text':'NetworkError'}
    except yandex_music.exceptions.TimedOutError:
        return {'responce':'error', 'text':'TimedOutError'}

if __name__ == "__main__":
    print(my_wave())