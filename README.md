# FileChunker

Splits the given file to chunks of goven size, calculates the sha256 hash of each chunk and outputs result to console.

Run with command line arguments:
- AppSettings:FilePath=
- AppSettings:ChunkSize=

Tested with 40Gb file:
- 1Mb (40k chunks): ~70s 
-- ~400Mb RAM
-- 100% CPU + 30% SSD

- 100kb (400k chunks): ~100s
-- ~800Mb RAM
-- 100% CPU + 30% SSD 