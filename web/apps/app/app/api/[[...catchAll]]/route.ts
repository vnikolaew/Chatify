import { NextRequest, NextResponse } from "next/server";

process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0";

async function handler(req: NextRequest) {
   // Extract cookies from the incoming request
   req.cookies.getAll();

   // Define the URL of your backend server
   const backendUrl = process.env.NEXT_PUBLIC_BACKEND_API_URL;

   // Forward the request to the backend server
   console.log(req.nextUrl.pathname);
   try {
      const backendRes = await fetch(`${backendUrl}${req.nextUrl.pathname}`, {
         method: req.method,
         mode: `cors`,
         credentials: `include`,
         next: {
            revalidate: false,
         },
         headers: {
            ...req.headers,

            // Forward cookies to the backend server
            Cookie: req.cookies
               .getAll()
               .map(({ value, name }) => `${name}=${value}`)
               .join("; "),
         },
         body: req.method === "GET" || req.method === "HEAD" ? null : req.body,
      });

      console.log({ backendRes });
      backendRes.headers.forEach((value, key) =>
         console.log(`${key} -> ${value}`)
      );

      if (backendRes.headers.get("Content-Type") === `application/json`) {
         return NextResponse.json(await backendRes.json(), {
            status: backendRes.status,
            headers: new Headers(backendRes.headers),
         });
      }

      return NextResponse.json(
         {},
         {
            status: backendRes.status,
            headers: new Headers(backendRes.headers),
         }
      );
   } catch (err) {
      console.log(err);
      return NextResponse.json({}, { status: 404 });
   }
}

export const GET = handler;
export const POST = handler;
export const DELETE = handler;
export const PATCH = handler;
export const PUT = handler;
export const HEAD = handler;
export const OPTIONS = handler;
