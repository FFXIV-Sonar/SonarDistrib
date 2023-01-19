XoshiroPRNG.Net
===============

This library contains a **_subset_** of the [xoshiro/xoroshiro PRNG family](http://xoshiro.di.unimi.it/) researched & developed by Sebastiano Vigna and David Blackman.

Why a subset? Because I _think_ having less than 128 bits of PRNG state is close to useless[^1], and also because I don't believe there's any device capable of running the .Net Framework / Core which does not have enough resource to implement 128-bit state.

[^1]: It is known that to get _n_ random numbers, you need at least _n<sup>2</sup>_ states. A 64-bit state means that _at most_ you'll get 2<sup>32</sup> random numbers. This is _barely_ able to fulfill the requirements of `System.Random`. Rather than indirectly causing issues due to the limited state space, I decided to forego these PRNGs.

In other words, this library contains _only_ the PRNG functions with 256 or 128 bits of state.

I have hand-optimized the library so that it is at least as performant as `System.Random`, while also maintaining compatibility with `System.Random`'s interface, yet written purely using **safe C#** code operations. The checkbox "Allow unsafe code" remains unchecked ;)

IMPORTANT: If you want to "unleash" the performance of this library, you should NOT use `System.Random` interface and use the "Unleashed" interface instead. See below for details.

[**This library is available on NuGet.org as XoshiroPRNG.Net.**](https://www.nuget.org/packages/XoshiroPRNG.Net/) <== click

&nbsp;

Namespaces
----------

This library contains the following namespaces:

| Namespace | Description |
|-----------|-------------|
| `Xoshiro.Base` | Some public interfaces, a public enum, and a public class `SplitMix64`. **You'll need this if you want the "Unleashed" interface.**. |
| `Xoshiro.PRNG32` | Contains classes that **natively generate 32-bit values**. This is usually the class you want. |
| `Xoshiro.PRNG64` | Contains classes that **natively generate 64-bit values**. All of them can generate 32-bit values as well, but not as fast as the ones in the `Xoshiro.PRNG32` because they have to work internally in 64-bit, then they have to "fold" the result to 32-bit values. |

&nbsp;

Public Concrete Classes
-----------------------

| Class | Description |
|-------|-------------|
| `Xoshiro.Base.SplitMix64` | Generates a series of random(-ish) 64-bit values[^2]. Mostly used to seed the state of the xoshiro PRNGs. |
| `Xoshiro.PRNG32.XoShiRo128starstar` | Implementation of the `xoshiro128**` PRNG. |
| `Xoshiro.PRNG32.XoShiRo128plus` | Implementation of the `xoshiro128+` PRNG. |
| `Xoshiro.PRNG64.XoShiRo256starstar` | Implementation of the `xoshiro256**` PRNG. |
| `Xoshiro.PRNG64.XoShiRo256plus` | Implementation of the `xoshiro256+` PRNG. |
| `Xoshiro.PRNG64.XoRoShiRo128starstar` | Implementation of the `xoshiro128**` PRNG. |
| `Xoshiro.PRNG64.XoRoShiRo128plus` | Implementation of the `xoroshiro128+` PRNG. |

[^2]:SplitMix64 only has a 64-bit state space, so I don't consider that a satisfactory PRNG. It is included here because to generate a series of numbers for a PRNG's initial state, [it is _strongly recommended_ to use a totally different algorithm from the PRNG's, to prevent correlation that might weaken the output of the PRNG](https://dl.acm.org/citation.cfm?doid=1276927.1276928). SplitMix64 is sufficiently different _and_ has the wanted property of uniformly distributed avalanche function. That means even if an output of SplitMix64 results in all-zero, the next output will be wildly different, thus fulfilling the need of the Xoshiro PRNG Family, i.e., the initial state must not be "everywhere zero".

Which one to use? I suggest you read the [xoroshiro paper](http://xoshiro.di.unimi.it/) to be informed.

But my recommendation is:

* For 64-bit random integers & double precision floating-point: `XoShiRo256starstar`
* For ONLY double precision floating-point: `XoShiRo256plus`
* For 32-bit random integers & single precision floats: `XoShiRo128starstar`
* For ONLY single precision floats: `XoShiRo128plus`

**Benchmark results are welcome!** If you can setup a good benchmark, "a C# PRNG shootout" so to speak, please drop me a message via the Issues page!

&nbsp;

API - System.Random-compatible
------------------------------

I tried to maintain compatibility with the [interface of .Net System.Random class](https://docs.microsoft.com/en-us/dotnet/api/system.random?view=netframework-4.5.2#methods).

So, excluding the methods inherited from the Object class, all public classes of this library implement the following methods:

| Method | Purpose |
|--------|---------|
| `Next()` | Returns a non-negative random Int32 value.<br/>Please note that, similar to `System.Random`, this method will NOT return `Int32.MaxValue`, ever. If you need 'full-range', use `NextU()` and chop off the topmost bit then cast to `int`.  |
| `Next(Int32)` | Returns a non-negative random Int32 that is less than the specified maximum (`0 <= result < maxValue`) |
| `Next(Int32, Int32)` | Returns a random Int32 that is within a specified range (`minValue <= result < maxValue`)<br/>Note: The implementation of this method in this library IS able to return negative numbers, if wanted. |
| `NextBytes(Byte[])` | Fills the elements of a specified array of bytes with random numbers. |
| `NextDouble()` | Returns a random double precision floating-point number that is greater than or equal to 0.0, and less than 1.0. |

These are known as **`System.Random`-compatible Interface.**

Please note that due to the chopping off of the sign bit in the conversion from `uint` to `Int32`, the effective number of random bits for the first three methods are 31 bits instead of 32 bits.

&nbsp;

API - "Unleashed"
-----------------

"Unleashed" here means an interface that directly accesses the PRNG's generator routine, providing **maximum number of random bits** per call, and with less casting overhead.

In addition to the `System.Random`-compatible methods above, all PRNG classes have the following methods:

| Method | Purpose |
|--------|---------|
| `GetRandomCompatible()` | Returns an object whose interfaces are 100% identical with `System.Random`[^3] |
| `NextU()` | Returns a non-negative random uint value |
| `NextU(uint)` | Returns a non-negative random uint that is less than the specified maximum (`0 <= result < maxValue`) |
| `NextU(uint, uint)` | Returns a random uint that is within a specified range (`minValue <= result < maxValue`) |
| `NextFloat()` | Returns a random **single precision** floating-point number that is greater than or equal to 0.0, and less than 1.0. |

[^3]: Under normal circumstances, you really don't need this because the Xoshiro classes are subclassed from `System.Random`. However, should you be in a situation where a consumer of your PRNG _demands_ exactness to `System.Random`, you can use this method.

Also, all PRNG**64** classes have the following methods:

| Method | Purpose |
|--------|---------|
| `Next64()` | Returns a non-negative random **Int64** value.<br/>Unlike `Next()`, this method is full-range, i.e., it _includes_ `Int64.MaxValue` |
| `Next64U()` | Returns a non-negative random **ulong** value |
| `Next64(Int64)` | Returns a non-negative random **Int64** that is less than the specified maximum (`0 <= result < maxValue`) |
| `Next64U(ulong)` | Returns a non-negative random **ulong** that is less than the specified maximum (`0 <= result < maxValue`) |
| `Next64(Int64, Int64)` | Returns a random **Int64** that is within a specified range (`minValue <= result < maxValue`) |
| `Next64U(ulong, ulong)` | Returns a random **ulong** that is within a specified range (`minValue <= result < maxValue`) |

Similar to the situation w.r.t. Int32 and uint, all Int64 variants of the methods above actually has 63 bits of randomness instead of 64, because the sign bit gets chopped. So, use the "U" variants -- "U" here means "Unsigned" or, optionally, "Unleashed" ;-)

> **To use the "Unleashed" API, set the type to `IRandomU` or `IRandom64U`,** as can be seen in the Usage section below.

&nbsp;

Usage
-----

Because all PRNG classes inherit from `System.Random`, just use it in lieu of Random, as such:

- - -

```csharp
// Replace PRNGxx with the namespace you want to use
using Xoshiro.PRNGxx;

...
    // Replace Xoshiro_Class with the class you want to use
    Random rand = new Xoshiro_Class();
...

...
    int value = rand.Next();  // Or any other System.Random-compatible methods

```

- - -

and that's it! Because of the API compatibility, you don't have to do any other changes.

You only need to rewrite things **if you want to use the Unleashed APIs**, which can return values not generatable by the `System.Random` class, e.g., `ulong` values, `uint` values, `float`s, etc.

- - -

**Example using 32-bit PRNG in "Unleashed" mode:**

```csharp
using Xoshiro.Base;
using Xoshiro.PRNG32;

...
    IRandomU rand32 = new Xoshiro_32bit_Class();
...

...
    uint value1 = rand32.NextU();
    float value2 = rand32.NextFloat();
    Bytes[] arr1 = new Bytes[16];
    rand32.NextBytes(arr1);
...

```

**Example using 64-bit PRNG in "Unleashed" mode:**

```csharp
using Xoshiro.Base;
using Xoshiro.PRNG64;

...
    IRandom64U rand64 = new Xoshiro_64bit_Class();
...

...
    ulong value1 = rand64.Next64U();
    long value2 = rand64.Next64();
    float value3 = rand64.NextFloat();
    double value4 = rand64.NextDouble();
    Bytes[] arr1 = new Bytes[32];
    rand64.NextBytes(arr1);
...
```

- - -

There. Easy peasy, isn't it? :-)

&nbsp;

Reproducible PRNG Sequences
---------------------------

As you might have already known, **all PRNGs will produce the exact same sequence when given the same seed**. At a glance, this might be looked at as a security issue. However, _reproducibility_ is actually something _very_ important in some research fields. A notable example is [the Monte Carlo Methods/Simulation](https://www.scratchapixel.com/lessons/mathematics-physics-for-computer-graphics/monte-carlo-methods-in-practice/generating-random-numbers). I quote:

> ... If you use Monte Carlo methods to produce an image, a "repeatable" random number generator (it would pseudorandom then), allows to lock the noise pattern in the image. "Repeatability" of the results in a production environment, is **an absolute necessity**. ...
>
> To summarize "controllable and repeatable randomness" is generally what we are truly after.

(Emphasis mine.)

So, just like other PRNGs, the Xoshiro family allows truly "repeatable" randomness by specifying a starting seed.

For this purpose, XoshiroPRNG.Net provides three constructors (I replace the actual class's name with "Class" below):

| Constructor | Description |
|-------------|-------------|
| `Class()` | The null constructor pulls a `long` value from the system's counters<sup>[1]</sup> and calls `Class(long seed)` with that number as seed. |
| `Class(long seed)` | Uses a `long` (`Int64`) seed which gets fed to a `SplitMix64` to generate a "repeatable" initial state for the PRNG. |
| `Class(UIntXX[] initialState)` | Uses an array of `UIntXX` values (`XX` = `32` or `64` depending on the PRNG's class) to directly set the initial state for the PRNG.<br/>There needs to be enough elements in the array to populate the PRNG's initial state. The constructor will use only enough elements to populate the initial state, and no more. |

**Note [1]:** Up to version 1.2.4, this value comes from [`System.Diagnostics.Stopwatch.GetTimestamp()`](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.stopwatch.gettimestamp?view=netframework-4.5.2). Later versions (>1.2.4) use the value from [`System.DateTime.UtcNow.Ticks`](https://docs.microsoft.com/en-us/dotnet/api/system.datetime.ticks?view=netframework-4.5.2) instead. For the reasoning, see the next section.

If you need "repeatable" random number generation, nearly all the time you will want to use `Class(long seed)`, unless you _really need_ to set the initial values (e.g., you're actually doing a research on the Xoshiro PRNG Family itself.)

But for the great majority of uses, which do NOT need "repeatable" random numbers, you'll be served quite well with the `Class()` null constructor.

&nbsp;

On Why DateTime.UtcNow.Ticks is Good Enough&trade;
--------------------------------------------

Sure, we all probably know that [`DateTime.UtcNow` gets updated only about every 15ms or so](https://stackoverflow.com/a/19736972/149900). So, although `DateTime.UtcNow.Ticks` has a 0.1 microsecond _resolution_, its _precision_ is much less[^4].

[^4]: "Resolution" here means the smallest unit able to be expressed by a measure, while "Precision" here means the smallest 'spread' between two measurements.

But that lowered precision is **of no consequence** when viewed in the context of a PRNG seed, because a PRNG seed is not meant to measure time accurately.

What's _much_ more important is that `DateTime.UtcNow.Ticks` is a **monotonically increasing** counter, so that provided the timezone and date/time of the computer is not wildly incorrect, repeated calls to the null constructor of the PRNG classes _will_ always result in a different seed.

In contrast, `System.Diagnostics.Stopwatch.GetTimestamp()` has _both_ high resolution _and_ high precision... but also a non-zero chance of getting a duplicate, since it measures only how many ticks have passed since system start.

So, a 'guaranteed' different seed vs a 'non-guaranteed' different seed? I choose the former.

The lack of precision for `DateTime.UtcNow.Ticks` should not be a problem unless you're starting a bunch of (virtual) machines at the very same instant and every one of them instantiates the exact same class. In which case, the odds of having a seed collision greatly increase.

In such situation, I have some suggested solutions:

1. Mix in the machine's identity, e.g. its IP address, into the value of `DateTime.UtcNow.Ticks`. Either mix the 32-bit IPv4 address to the upper part of the seed, or mix the lower 64-bit of IPv6 address over the whole seed. Alternatively, use the machine's MAC address.

2. Pull in a True RNG value over the network, e.g. from [Australian National University's Quantum Random Number Server](https://qrng.anu.edu.au/), and use that as seed. These sources are as random as possible and have much MUCH smaller chance of ever generating a duplicate. (You may get throttled by such service though if you pulled too many random numbers within a short period of time.)

3. Prepare a Unix (virtual) machine (e.g., running *BSD or Linux) and write a simple web server that, when accessed, returns 8 bytes from `/dev/urandom`. Have the virtual machines with XoshiroPRNG.Net pull their seeds from that server.

Anyways, such a situation where `DateTime.UtcNow.Ticks` might collide between a large bunch of virtual machines seems for me to be a "heavyweight class" situation, and I do believe you should have enough resources to solve the problem ;)

&nbsp;

License
-------

**The original** code has been released under Public Domain or [CC0](https://spdx.org/licenses/CC0-1.0.html#licenseText) license.

Therefore, I'm releasing this code under the same -- or similar -- license(s).

Choose one that's applicable for your jurisdiction & needs:

* Public Domain
* [The Unlicense](https://spdx.org/licenses/Unlicense.html)
* [CC0](https://spdx.org/licenses/CC0-1.0.html#licenseText)
* [WTFPL](https://spdx.org/licenses/WTFPL.html#licenseText)
* [MIT-0](https://spdx.org/licenses/MIT-0.html#licenseText)
* [BSD-3-Clause](https://spdx.org/licenses/BSD-3-Clause.html#licenseText)

If for some extraordinary reason (!?) you cannot use _any_ of the above, please contact me on **pepoluan (at sign) gmail (period) com** and we can arrange a license suitable for your needs.

&nbsp;

FAQ
---

**Why did you create this library?**

> I was looking for a good PRNG for a C# project I'm working on. The .Net Framework implements a [Subtractive Generator](http://rosettacode.org/wiki/Subtractive_generator) which has many weaknesses [\[1\]](https://fuglede.dk/en/blog/bias-in-net-rng/)[\[2\]](https://gist.github.com/fuglede/772402ecc3997ada82a03ce65361781b)[\[3\]](https://github.com/dotnet/corefx/issues/23298). Looking around, a better (and commonly used) PRNG is the [Mersenne Twister (MT)](https://en.wikipedia.org/wiki/Mersenne_Twister). Unfortunately, the library I found for MT has a license I'm not comfortable with to be using in my project.
>
> After some spelunking I found out about the [Xorshift family](https://en.wikipedia.org/wiki/Xorshift) of PRNGs, and from there I found the [xoshiro/xoroshiro family](http://xoshiro.di.unimi.it/) with even better performance (speed-wise and randomness-wise). And the icing on the cake, the xoshiro/xoroshiro family is released into Public Domain!

**Is there not already a library implementing this?**

> Not inside the NuGet library. There is [one NuGet package that implements xoroshiro128+](https://www.nuget.org/packages/NotEnoughTime.Utils.Random/), but ~~it does not implement the other PRNGs, and furthermore~~ its interface ~~is not compatible with~~ albeit having similar methods, is not a drop-in for `System.Random`.
>
> (Also, that library [1] uses _unsafe_ code, and [2] does NOT implement any 32-bit outputting generators.)
>
> Even searching for C# source code files for the xoshiro/xoroshiro family will turn up .cs files that are either (1) not compatible with `System.Random` interface, or (2) not released with a suitable license, or (3) both.

**Is this library fast?**

> I have done some non-exhaustive tests of the algorithms, and every one of them is at least as fast -- if not actually faster, in some cases _much_ faster -- than `System.Random`. Especially when compiled for `x64` (that is, 64-bit) CPU architecture.

**Why do you release this into the Public Domain / with a very permissive license?**

> Messrs. Vigna & Blackman had been kind enough to release their algorithms and their reference code into the Public Domain.
>
> I'm just a repackager, I stand on the shoulders of giants. What am I to impose something more stringent if the creators had released things for free?

**But I want to give you a reward!**

> Just promise to be a good person. Pay it forward.

**But I _really_ want to give you a reward!**

> Weeeeellll... I _do_ have [**a Patreon page**](https://www.patreon.com/pepoluan) ...


&nbsp;

Archival Links
--------------

Wayback Machine snapshots of important page:

* [xoshiro / xoroshiro generators and the PRNG shootout](http://xoshiro.di.unimi.it/) -- [(archived)](https://web.archive.org/web/20190706185419/http://xoshiro.di.unimi.it/)
* [xoshiro256** source](http://xoshiro.di.unimi.it/xoshiro256starstar.c) -- [(archived)](https://web.archive.org/web/20190706185530/http://xoshiro.di.unimi.it/xoshiro256starstar.c)
* [xoshiro256+ source](http://xoshiro.di.unimi.it/xoshiro256plus.c) -- [(archived)](https://web.archive.org/web/20190706185606/http://xoshiro.di.unimi.it/xoshiro256plus.c)
* [xoroshiro128** source](http://xoshiro.di.unimi.it/xoroshiro128starstar.c) -- [(archived)](https://web.archive.org/web/20190706185647/http://xoshiro.di.unimi.it/xoroshiro128starstar.c)
* [xoroshiro128+ source](http://xoshiro.di.unimi.it/xoroshiro128plus.c) -- [(archived)](https://web.archive.org/web/20190706185722/http://xoshiro.di.unimi.it/xoroshiro128plus.c)
* [xoshiro128** source](http://xoshiro.di.unimi.it/xoshiro128starstar.c) -- [(archived)](https://web.archive.org/web/20190706185824/http://xoshiro.di.unimi.it/xoshiro128starstar.c)
* [xoshiro128+ source](http://xoshiro.di.unimi.it/xoshiro128plus.c) -- [(archived)](https://web.archive.org/web/20190706185908/http://xoshiro.di.unimi.it/xoshiro128plus.c)
