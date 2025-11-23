# Ongoing thoughts and notes whilst working on this assessment

## Blog Posts & Caching

- Use of in-memory caching for blog posts.

This would negate any repeated IO operations.
Give a realistic cache policy (10 minutes?) - preference dependent on desire for up-to-date content vs performance.
Perhaps consider cache invalidation on file change (new comment) - NOT GREAT in real world, as you'll forever be invalidating cache on any well-trafficked sites.
Future consideration would be to split blogs posts into individual files, bigger sites would want comments seperated to allow for the post to still be cached.

- Changed JSON retrieval to IWebHostEnvironment's ContentRootPath

Much better than storing in wwwroot - cannot be accessed by client browsers.

## Adding Comments

- POST Endpoint

Works nicely.
Currently forces a page refresh - would be nice to change this to a call via Fetch (TODO)
Added a simple semaphore to limit concurrent writes to the JSON file.
Noticed that JSON is written back with Unicode encoding for HTML characters - "safer" but discussions around policies for XSS would be needed in a real world app.
