"use client";
import React from "react";
import {
   Card,
   CardBody,
   CardFooter,
   CardHeader,
   Link,
   Image,
   Divider,
} from "@nextui-org/react";
import * as NextLink from "next/link";

const ITEMS = [
   {
      title: "Orange",
      img: "/images/fruit-1.jpeg",
      price: "$5.50",
   },
   {
      title: "Tangerine",
      img: "/images/fruit-2.jpeg",
      price: "$3.00",
   },
   {
      title: "Raspberry",
      img: "/images/fruit-3.jpeg",
      price: "$10.00",
   },
   {
      title: "Lemon",
      img: "/images/fruit-4.jpeg",
      price: "$5.30",
   },
   {
      title: "Avocado",
      img: "/images/fruit-5.jpeg",
      price: "$15.70",
   },
   {
      title: "Lemon 2",
      img: "/images/fruit-6.jpeg",
      price: "$8.00",
   },
   {
      title: "Banana",
      img: "/images/fruit-7.jpeg",
      price: "$7.50",
   },
   {
      title: "Watermelon",
      img: "/images/fruit-8.jpeg",
      price: "$12.20",
   },
];

const CardsPage = () => {
   return (
      <div className={`flex m-4 w-max flex-col gap-8`}>
         <Card className={`shadow-large hover:scale-[103%]`}>
            <CardHeader className={`flex p-4 gap-3`}>
               <Image
                  radius={"lg"}
                  height={50}
                  width={50}
                  src={`https://i.pravatar.cc/150?u=a04258114e29026708c`}
                  alt={`some image`}
               />
               <div className={`flex justify-around h-full flex-col gap-2`}>
                  <h2 className={`text-xl`}>Header</h2>
                  <Link
                     className={`text-default-500 text-sm`}
                     href={`https://nextui.org/docs`}
                  >
                     Go to docs
                  </Link>
               </div>
            </CardHeader>
            <Divider />
            <CardBody>
               <p className={`font-light`}>
                  Make beautiful websites regardless of your design experience.
               </p>
            </CardBody>
            <Divider />
            <CardFooter>
               <Link
                  showAnchorIcon={true}
                  href={`https://github.com/nextui-org/nextui`}
                  isExternal={true}
               >
                  View source code on GitHub.
               </Link>
            </CardFooter>
         </Card>
         <Card className={`col-span-12 max-w-[500px] h-[300px]`}>
            <CardHeader className={`absolute z-10 top-1 flex-col !items-start`}>
               <p className={`text-tiny text-white/60 uppercase font-bold`}>
                  Plant a tree
               </p>
               <h4 className={`text-white font-medium text-large`}>
                  Contribute to the planet
               </h4>
            </CardHeader>
            <Image
               removeWrapper
               alt={`Card background`}
               className={`z-0 w-full h-full object-cover`}
               src={`https://nextui.org/images/card-example-3.jpeg`}
            />
         </Card>
         <div className={`mt-8 grid grid-cols-4 gap-4`}>
            {ITEMS.map((item, i) => (
               <Card
                  key={i}
                  shadow={"sm"}
                  className={`max-w-[200px]`}
                  onPress={(_) => console.log(`Pressed`)}
                  isPressable
               >
                  <CardBody className={`overflow-visible p-0`}>
                     <Image
                        className={`w-full object-cover h-[140px]`}
                        alt={"alt text"}
                        src={`https://nextui.org${item.img}`}
                        width={"100%"}
                        radius={"lg"}
                        shadow={`sm`}
                     />
                  </CardBody>
                  <CardFooter className={`text-small justify-between`}>
                     <b>{item.title}</b>
                     <p className={`text-default-500`}>{item.price} </p>
                  </CardFooter>
               </Card>
            ))}
         </div>
         <NextLink.default
            className={`hover:underline ml-auto self-end text-xl text-blue-500`}
            href={`/_playgrounds`}
         >
            Go Back
         </NextLink.default>
      </div>
   );
};

export default CardsPage;
