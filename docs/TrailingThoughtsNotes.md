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

## Adding Replies to Comments

How big of a comment tree should we allow?

We can keep the data structure the same as normal comments - maybe limit to 2 deep for formatting?

In larger systems we absolutely could look at a "view more" option like social media sites do (Reddit) when trees get deep.


We need a way to uniquely ident. comments to tag replies to - will add a guid.

## Adding Comments w/out Page Refresh

As previous, wanted to perform AJAX call for comments.

Typically, you'll see people receive the JSON for the new data, and construct the HTML client-side.

I wanted to explore the server rendering this, whilst the construction still occurs in JS (and could be modified), I quite liked the abstraction this provided - especially being able to strongly reference the partial view.

I suppose where on page load, you get the type safety when initially constructing the comments, you lose that when adding new comments via JS, so keeping it consistent feels right.

Practically, this wouldn't be easily scalable to large sites where users would like to render 1000s of new comments a second.

## File Uploads

- Basic File Uploads

For this, we can store file uploads in wwwroot, so that they can be publically accessed and served by the web server.

Real world considerations would be to store these in a CDN/S3/Blob Storage etc. for scalability, performance, security. Also Virus scanning (VirusTotal, ClamAV etc.) and file type validation.