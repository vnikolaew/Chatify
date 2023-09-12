"use client";
import React from "react";
import {
   ModalBody,
   Button,
   ModalContent,
   ModalHeader,
   ModalProps,
   Modal,
} from "@nextui-org/react";
import { X } from "lucide-react";

export interface ToastProps extends ModalProps {
   header?: React.ReactNode;
}

const Toast = ({ children, header, ...props }: ToastProps) => {
   return (
      <Modal
         backdrop={"transparent"}
         classNames={{
            base: "absolute text-xs sm:my-0 my-0 bg-default-300 opacity-90 bottom-10 right-10 w-fit min-w-fit max-w-fit mx-auto",
         }}
         shadow={"md"}
         closeButton={
            <Button
               size={"sm"}
               radius={"full"}
               variant={"light"}
               isIconOnly
               className={`top-2 bg-transparent p-0 right-2`}
               // color={"default"}
            >
               <X className={`fill-white stroke-white`} size={12} />
            </Button>
         }
         radius={"sm"}
         motionProps={{
            variants: {
               enter: {
                  y: 0,
                  opacity: 1,
                  transition: {
                     duration: 0.3,
                     ease: "easeOut",
                  },
               },
               exit: {
                  y: 20,
                  opacity: 0,
                  transition: {
                     duration: 0.2,
                  },
               },
            },
         }}
         size={"md"}
         placement={"bottom"}
         {...props}
      >
         <ModalContent>
            <ModalHeader className={`pb-0`}>{header}</ModalHeader>
            <ModalBody className={`pb-4`}>{children}</ModalBody>
         </ModalContent>
      </Modal>
   );
};

export default Toast;
