"""Recreate only the thirteen original prototype WAV assets; no samples or recordings.

Run from any directory using Python 3 standard library. Asset GUID metas are preserved.
These waveforms are production placeholders, not a licensed soundtrack.
"""
from pathlib import Path
import math
import random
import struct
import wave

OUTPUT = Path(__file__).resolve().parents[1] / 'Assets' / 'Data' / 'Audio'
NAMES = ('Attack', 'Hit', 'Dash', 'Parry', 'Enemy', 'Rescue', 'Honmoon',
         'Exploration', 'Combat', 'Combo', 'Crowd', 'Crisis', 'Special')
RATE = 22050


def synthesize(name, index):
    loop = index >= 7
    duration = 4 if loop else .18 + .025 * index
    frames = bytearray()
    rng = random.Random(78)
    noise = 0.0
    for i in range(int(RATE * duration)):
        t = i / RATE
        if name == 'Crowd':
            noise = noise * .92 + (rng.random() * 2 - 1) * .08
            clap = max(0, 1 - (t % .5) / .06) ** 2
            sample = (.11 * noise + .08 * clap * (rng.random() * 2 - 1)
                      + .012 * math.sin(2 * math.pi * 165 * t)
                      * (1 + .5 * math.sin(2 * math.pi * t)))
        else:
            frequency = 110 + 55 * (index - 7) if loop else 220 + 80 * index
            envelope = (.35 + .2 * math.sin(2 * math.pi * 2 * t)
                        if loop else (1 - t / duration) ** 2)
            edge = min(1, t / .02, (duration - t) / .02)
            sample = .10 * envelope * edge * (
                math.sin(2 * math.pi * frequency * t)
                + .25 * math.sin(2 * math.pi * frequency * 2 * t))
        frames.extend(struct.pack('<h', int(sample * 32767)))
    with wave.open(str(OUTPUT / (name + '_OriginalPrototype.wav')), 'wb') as stream:
        stream.setnchannels(1)
        stream.setsampwidth(2)
        stream.setframerate(RATE)
        stream.writeframes(frames)


if __name__ == '__main__':
    OUTPUT.mkdir(parents=True, exist_ok=True)
    for index, name in enumerate(NAMES):
        synthesize(name, index)
