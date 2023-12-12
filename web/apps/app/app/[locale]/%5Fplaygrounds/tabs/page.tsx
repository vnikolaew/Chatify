"use client";
import React from "react";
import Link from "next/link";
import { Card, CardBody, Tab, Tabs } from "@nextui-org/react";
import { LOREM_IPSUM } from "../modals/page";

const TabsPage = () => {
   const variants = ["solid", "underlined", "bordered", "light"];
   return (
      <div className={`flex m-8 w-1/3 flex-col gap-8`}>
         <div className={`flex flex-col items-center gap-4`}>
            {variants.map((v, i) => (
               <Tabs
                  key={i}
                  variant={v as any}
                  color={"primary"}
                  size={"lg"}
                  disabledKeys={["tab-2"]}
               >
                  <Tab className={`text-large`} title={"Tab 1"} key={"tab-1"}>
                     <Card className={`text-large`}>
                        <CardBody>{LOREM_IPSUM}</CardBody>
                     </Card>
                  </Tab>
                  <Tab className={`text-large`} title={"Tab 2"} key={"tab-2"}>
                     <Card className={`text-large`}>
                        <CardBody>{LOREM_IPSUM}</CardBody>
                     </Card>
                  </Tab>
                  <Tab className={`text-large`} title={"Tab 3"} key={"tab-3"}>
                     <Card className={`text-large`}>
                        <CardBody>{LOREM_IPSUM}</CardBody>
                     </Card>
                  </Tab>
               </Tabs>
            ))}
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

export default TabsPage;
