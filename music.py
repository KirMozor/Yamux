from yandex_music import Best, Client, Search
import yandex_music
import toml
config = toml.load("config.toml")

client = Client(config.get('token_yandex')).init()

type_to_name = {
    'track': 'трек',
    'artist': 'исполнитель',
    'album': 'альбом',
    'playlist': 'плейлист',
    'video': 'видео',
    'user': 'пользователь',
    'podcast': 'подкаст',
    'podcast_episode': 'эпизод подкаста',
}

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

def info_track(url):
    url_parts=url.split('/')
    track_id = url_parts[-1]
    album_id = url_parts[-3]
    track = client.tracks([track_id])[0]
    album = client.albums([album_id])[0]

    artists = ""
    for i in track.artists_name():
        artists = artists + f" {i}"
    s = {'name' : f'{track.title}', 'artists' : f'{artists}', 'album' : f'{album.title}', 'genre' : f'{album.genre}', 'duration' : f'{track.duration_ms / 1000}'}
    return s

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

def send_search_request_and_print_result(query, what_to_search):
    search_result = client.search(query)

    if search_result.best and what_to_search == "best":
        return search_result.best
    if search_result.albums and what_to_search == "albums":
        return search_result.albums
    if search_result.tracks and what_to_search == "tracks":
        return search_result.tracks
    if search_result.playlists and what_to_search == "playlists":
       return search_result.playlists
    if search_result.videos and what_to_search == "videos":
        return search_result.videos

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
    result = send_search_request_and_print_result("Skrillex", "albums")
    for i in result.results:
        id_album = i.id
        print(client.albums_with_tracks(id_album).id)
