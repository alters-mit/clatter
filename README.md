# Clatter

**Clatter is a C# library that can synthesize plausible sounds from physics-driven events.** Given a collision, the mass of the two objects, their "audio materials", the relative velocity, and so on, Clatter will generate a unique sound. Currently, Clatter is capable of generating impact and scrape sounds.

**TODO add documentation link**

**TODO add license link**

## Features

- Unique and powerful audio synthesis capabilities. Unlike most physics driven audio generators, Clatter generates audio out of scientifically accurate, pre-computed data, rather than relying on modifying or mixing pre-existing audio files.
- Built-in support for Unity.
- Highly performant. Clatter uses efficient memory management and multithreaded processes to generate audio.
- An intuitive, flexible API.

## Roadmap

Pre-alpha to-dos:

- [ ] Add documentation.
- [ ] Add releases and build tools.
- [ ] Add Python bindings for `Clatter.Core.dll`.
- [x] Add command-line tools for OS X and Linux.
- [ ] Add Clatter to TDW.
- [ ] Add links to Clatter/TDW documentation
- [ ] Add a license.
- [x] Add attribution.

Clatter is currently in alpha. It is likely still buggy and unstable. The API will likely change often until the first stable release.

- We intend to support rolling sounds but this hasn't yet been implemented.
- Softer impact materials such as "fabric" don't sound as good as harder materials. This will be corrected in the near-future.
- Impacts tend to sound best for small objects. We're working on adding better support for larger objects.
- Some scrape materials may sound distorted or "scratchy". We'll fix these.

## Attribution

Clatter was developed by Esther Alter.

Clatter was originally a C# port of [PyImpact](https://github.com/threedworld-mit/tdw/blob/master/Documentation/lessons/audio/py_impact.md), a component of [ThreeDWorld](https://github.com/threedworld-mit/tdw), a research simulation platform. Clatter has since expanded upon the PyImpact's functionality.

PyImpact was developed by James Traer, Maddie Cusimano, Josh McDermott, Vin Agarwal, and Esther Alter. When using Clatter for research, please cite [Traer,Cusimano  and McDermott, A perceptually inspired generative model of rigid-body  contact sounds, Digital Audio Effects, (DAFx), 2019](http://dafx2019.bcu.ac.uk/papers/DAFx2019_paper_57.pdf) and [Agarwal,  Cusimano, Traer, and McDermott, Object-based synthesis of scraping and  rolling sounds based on non-linear physical constraints, (DAFx), 2021](http://mcdermottlab.mit.edu/bib2php/pubs/makeAbs.php?loc=agarwal21).

```
@article {4500,
	title = {A perceptually inspired generative model of rigid-body contact sounds},
	journal = {Proceedings of the 22nd International Conference on Digital Audio Effects (DAFx-19)},
	year = {2019},
	month = {09/2019},
	abstract = {<p>Contact between rigid-body objects produces a diversity of impact and friction sounds. These sounds can be synthesized with detailed simulations of the motion, vibration and sound radiation of the objects, but such synthesis is computationally expensive and prohibitively slow for many applications. Moreover, detailed physical simulations may not be necessary for perceptually compelling synthesis; humans infer ecologically relevant causes of sound, such as material categories, but not with arbitrary precision. We present a generative model of impact sounds which summarizes the effect of physical variables on acoustic features via statistical distributions fit to empirical measurements of object acoustics. Perceptual experiments show that sampling from these distributions allows efficient synthesis of realistic impact and scraping sounds that convey material, mass, and motion.</p>
},
	author = {James Traer and Maddie Cusimano and Josh H. McDermott}
}
```

```
@inproceedings{agarwal21,
     TITLE= "Object-based synthesis of scraping and rolling sounds based on non-linear physical constraints",
     AUTHOR= "V Agarwal and M Cusimano and J Traer and J H McDermott",
     booktitle= "The 24th International Conference on Digital Audio Effects (DAFx-21)",
     MONTH= "September",
     YEAR= 2021,
     PDF-URL= "http://mcdermottlab.mit.edu/papers/Agarwal_etal_2021_scraping_rolling_synthesis_DAFx.pdf",
}
```
