"use client";
import React, { useState } from "react";
import Link from "next/link";
import { Button, Card, CardHeader, Skeleton } from "@nextui-org/react";

const SkeletonPage = () => {
   const [isLoaded, setIsLoaded] = useState(false);

   return (
      <div className={`flex m-8 w-1/3 flex-col gap-8`}>
         <div className={`flex flex-col items-center gap-4`}>
            <Card radius={"lg"} className={`space-y-5 p-4 w-[400px]`}>
               <Skeleton isLoaded={isLoaded} className={`rounded-lg`}>
                  <div className={`h-24 rounded-lg bg-danger-500`}></div>
               </Skeleton>
               <div className={`space-y-3`}>
                  <Skeleton
                     isLoaded={isLoaded}
                     className={`w-3/5 h-3 rounded-lg`}
                  >
                     <div className={`w-full h-3 rounded-lg bg-danger`}></div>
                  </Skeleton>
                  <Skeleton
                     isLoaded={isLoaded}
                     className={`w-4/5 h-3 rounded-lg`}
                  >
                     <div className={`w-full h-3 rounded-lg bg-danger`}></div>
                  </Skeleton>
                  <Skeleton
                     isLoaded={isLoaded}
                     className={`w-2/5 h-3 rounded-lg`}
                  >
                     <div className={`w-full h-3 rounded-lg bg-danger`}></div>
                  </Skeleton>
               </div>
            </Card>
            <Button
               color={"warning"}
               className={`w-max`}
               onClick={(_) => setIsLoaded(!isLoaded)}
            >
               Load items
            </Button>
         </div>
         <Link
            className={`hover:underline ml-auto self-end text-xl text-blue-500`}
            href={`/_playgrounds`}
         >
            Go Back
         </Link>
      </div>
   );
};

export default SkeletonPage;
