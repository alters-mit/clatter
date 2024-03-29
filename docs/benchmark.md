# Benchmark

To benchmark Clatter, compile and run the Clatter.Benchmark project.

There are two benchmarks:

- The impact benchmark generates a series of 100 impact sounds and returns the total elapsed time in seconds.
- The scrape benchmark generates a scrape that is 10 seconds long and returns the total elapsed time in seconds.
- The threaded benchmark generates 100 trials. Per trial, it creates 200 objects and generates 100 impact audio sounds. Audio generation is multi-threaded (via `AudioGenerator`). This returns two results: the total time elapsed, and the average time elapsed per trial. The average time can be compared to the impact benchmark.

**RESULTS:**

| Benchmark | Time (seconds) |
| --- | --- |
| Impact | 0.5707888 |
| Scrape | 1.4111531 |
| Threaded (total) | 11.010226299999992 |
| Threaded (average) | 0.11010226299999992 |