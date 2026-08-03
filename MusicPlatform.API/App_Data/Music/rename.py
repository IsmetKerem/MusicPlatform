import os, re, unicodedata

TR = str.maketrans("çÇğĞıİöÖşŞüÜ", "cCgGiIoOsSuU")

JUNK = [
    r"\(official.*?\)", r"\(prod.*?\)", r"\(.*?video.*?\)",
    r"\(.*?visualizer.*?\)", r"\(.*?versiyon.*?\)", r"\(.*?live.*?\)",
    r"official music video", r"official video", r"official visualizer",
    r"official backstage video",
]

for f in sorted(os.listdir(".")):
    if not f.lower().endswith(".mp3"):
        continue
    name = f[:-4]
    low = name.lower()
    for j in JUNK:
        low = re.sub(j, "", low)
    low = low.translate(TR)
    low = unicodedata.normalize("NFKD", low).encode("ascii", "ignore").decode()
    low = re.sub(r"[^a-z0-9]+", "-", low).strip("-")
    new = low + ".mp3"
    if new != f and not os.path.exists(new):
        os.rename(f, new)
        print(f"{f}  ->  {new}")

print("\n--- SON LİSTE ---")
for f in sorted(os.listdir(".")):
    if f.lower().endswith(".mp3"):
        print(f)