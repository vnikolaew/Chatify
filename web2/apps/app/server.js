const { createServer } = require("https");
const { parse } = require("url");
const next = require("next");
const { readFileSync } = require("fs");
const { join } = require("path");

const dev = process.env.NODE_ENV !== "production";

const hostname = process.env.HOST || "localhost";
const port = process.env.PORT ? parseInt(process.env.PORT) : 4200;

async function server() {
   const nextApp = next({ dev });
   const handle = nextApp.getRequestHandler();

   const certsFolder = join(__dirname, "certs");

   const httpsOptions = {
      key: readFileSync(join(certsFolder, "localhost-key.pem")),
      cert: readFileSync(join(certsFolder, "localhost.pem")),
   };

   await nextApp.prepare();
   const server = createServer(httpsOptions, (req, res) => {
      const parsedUrl = parse(req.url, true);
      handle(req, res, parsedUrl);
   });

   server.listen(port, hostname);
   console.log(`~> 🚀 Server started on https://${hostname}:${port}`);
}

server().catch((err) => {
   console.error(err);
   process.exit(1);
});
