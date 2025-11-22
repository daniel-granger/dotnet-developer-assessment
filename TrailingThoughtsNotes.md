# Ongoing thoughts and notes whilst working on this assessment

## Blog Posts & Caching

- Use of in-memory caching for blog posts.

This would negate any repeated IO operations.
Give a realistic cache policy (10 minutes?) - preference dependent on desire for up-to-date content vs performance.
Perhaps consider cache invalidation on file change (new comment) - NOT GREAT in real world, as you'll forever be invalidating cache on any well-trafficked sites.
Future consideration would be to split blogs posts into individual files, bigger sites would want comments seperated to allow for the post to still be cached.