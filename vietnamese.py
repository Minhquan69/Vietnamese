from flask import Flask, request, jsonify
import pyodbc
import requests
from youtube_transcript_api import YouTubeTranscriptApi

app = Flask(__name__)


def get_youtube_title(video_id):
    url = f"https://www.youtube.com/oembed?url=https://www.youtube.com/watch?v={video_id}&format=json"
    try:
        response = requests.get(url)
        if response.status_code == 200:
            return response.json()['title']
    except Exception as e:
        print("Title error:", e)

    return video_id


def insert_video(video_id, created_by):

    conn = pyodbc.connect(
        "DRIVER={ODBC Driver 17 for SQL Server};"
        "SERVER=localhost;"
        "DATABASE=vietnamese;"
        "Trusted_Connection=yes;"
    )

    cursor = conn.cursor()
    title = get_youtube_title(video_id)

    try:
        transcript = YouTubeTranscriptApi().fetch(video_id, languages=['vi'])
    except Exception as e:
        print("Transcript error:", e)
        transcript = []

    cursor.execute("""
    IF NOT EXISTS (SELECT 1 FROM Videos WHERE YoutubeId = ?)
    BEGIN
        INSERT INTO Videos (YoutubeId, Title, CreatedBy)
        VALUES (?, ?, ?)
    END
    """, (video_id, video_id, title, created_by))

    conn.commit()

    cursor.execute(
        "SELECT VideoId FROM Videos WHERE YoutubeId = ?",
        (video_id,)
    )

    row = cursor.fetchone()

    if not row:
        cursor.close()
        conn.close()
        return "Video not found"

    db_video_id = row[0]

    cursor.execute(
        "DELETE FROM Transcripts WHERE VideoId = ?",
        (db_video_id,)
    )

    conn.commit()

    data = []

    for line in transcript:
        data.append((
            db_video_id,
            line.text,
            float(line.start)
        ))

    if len(data) > 0:
        cursor.fast_executemany = True

        cursor.executemany("""
            INSERT INTO Transcripts (VideoId, Sentence, StartTime)
            VALUES (?, ?, ?)
        """, data)

        conn.commit()

    cursor.close()
    conn.close()

    return title

@app.route('/crawl', methods=['POST'])
def crawl():

    try:
        data = request.get_json()

        print("Request:", data)

        if not data:
            return jsonify({"error": "No JSON received"}), 400

        video_id = data.get('youtubeId')
        created_by = data.get('createdBy')

        if not video_id or not created_by:
            return jsonify({"error": "Missing youtubeId or createdBy"}), 400

        title = insert_video(video_id, created_by)

        return jsonify({
            "status": "success",
            "title": title
        })

    except Exception as e:
        print("ERROR:", e)

        return jsonify({
            "status": "error",
            "message": str(e)
        }), 500

if __name__ == '__main__':
    app.run(port=5001, debug=True)