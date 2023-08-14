"use client";
import React from "react";
import {
   Button,
   Input,
   Link,
   Popover,
   PopoverContent,
   PopoverProps,
   PopoverTrigger,
} from "@nextui-org/react";

const PopoverPage = () => {
   const colors: PopoverProps["color"][] = [
      "success",
      "default",
      "primary",
      "danger",
      "secondary",
   ];

   return (
      <div className={`flex min-h-[150vh] m-8 flex-col items-center gap-8`}>
         <div className={`flex-col w-full flex gap-4`}>
            <Popover showArrow placement={"right"}>
               <PopoverTrigger className={`w-min`}>
                  <Button color={"primary"}>Open Popover</Button>
               </PopoverTrigger>
               <PopoverContent>
                  <div className={`px-1 py-2`}>Content</div>
               </PopoverContent>
            </Popover>
            {colors.map((c, i) => (
               <Popover offset={10} color={c} showArrow placement={"right"}>
                  <PopoverTrigger className={`w-min`}>
                     <Button color={c as any}>Open Popover</Button>
                  </PopoverTrigger>
                  <PopoverContent>
                     <div className={`px-1 py-2`}>Content</div>
                  </PopoverContent>
               </Popover>
            ))}
            <Popover showArrow placement={"right"} backdrop={"blur"}>
               <PopoverTrigger className={`w-min`}>
                  <Button color={"default"}>Click me</Button>
               </PopoverTrigger>
               <PopoverContent>
                  <p className={`my-3 text-lg`}>Edit</p>
                  <div className={`flex-col flex items-start gap-2`}>
                     <Input
                        placeholder={"Enter a width"}
                        variant={"faded"}
                        label={"Width"}
                        type={"text"}
                     />
                     <Input
                        placeholder={"Enter a height"}
                        variant={"faded"}
                        label={"Height"}
                        type={"text"}
                     />
                  </div>
               </PopoverContent>
            </Popover>
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

export default PopoverPage;
