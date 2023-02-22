# Caching

Sylves grids are generally completely stateless, which makes them fairly lightweight objects, there are some exceptions.

In some cases, grids need to store some additional information for efficiency. However, as some grids are infinite, it's not always possible to compute it all upfront, and the amount of additional info could grow without limit. Classes that face this problem will accept a [`ICachePolicy`](xref:Sylves.ICachePolicy) which allows you to customize the long term storage of this info.

The default policy is [`CachePolicy.Always`](xref:Sylves.CachePolicy.Always) which means that all lazily generated information is kept indefinitely. But you could supply alternative implementations that implement a [LRU cache](https://en.wikipedia.org/wiki/Least_recently_used) or [arena allocation](https://en.wikipedia.org/wiki/Region-based_memory_management). A grid is associated with ever cache, so it should even be possible to make a cache that keeps data near the player, and drops anything too far away.


Caching should not be confused with the more general problem of storing user-data per cell, that is discussed in the [Storage](storage.md) article.