"use client";
import React, { Fragment } from "react";
import {
   Button,
   Checkbox,
   Input,
   Modal,
   ModalBody,
   ModalContent,
   ModalFooter,
   ModalHeader,
   ModalProps,
   Link,
   useDisclosure,
} from "@nextui-org/react";

export const LOREM_IPSUM =
   "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam pulvinar risus non risus hendrerit venenatis. Pellentesque sit amet hendrerit risus, sed porttitor quam.";

const ModalsPage = () => {
   const { onOpen, isOpen, onOpenChange } = useDisclosure();
   const {
      onOpen: loginOpen,
      isOpen: isLoginOpen,
      onOpenChange: onLoginOpenChange,
   } = useDisclosure();
   const sizes: ModalProps["size"][] = ["sm", "md", "lg", "xl", "2xl"];

   return (
      <div className={`flex m-8 flex-col items-center gap-8`}>
         <div className={`flex-col flex gap-4`}>
            <Button onPress={(_) => onOpen()}>Open Modal</Button>
            <Modal
               scrollBehavior={"inside"}
               backdrop={"opaque"}
               size={"3xl"}
               onOpenChange={onOpenChange}
               isOpen={isOpen}
            >
               <ModalContent>
                  {(onClose) => (
                     <>
                        <ModalHeader>Title</ModalHeader>
                        <ModalBody>
                           {[1, 2, 3].map((i) => (
                              <p key={i}>{LOREM_IPSUM}</p>
                           ))}
                        </ModalBody>
                        <ModalFooter>
                           <Button
                              color="danger"
                              variant="shadow"
                              onClick={onClose}
                           >
                              Close
                           </Button>
                           <Button color="primary" onPress={onClose}>
                              Action
                           </Button>
                        </ModalFooter>
                     </>
                  )}
               </ModalContent>
            </Modal>
         </div>
         <div>
            <Button color={"primary"} onPress={(_) => loginOpen()}>
               Login
            </Button>
            <Modal
               radius={"lg"}
               shadow={"md"}
               backdrop={"blur"}
               scrollBehavior={"inside"}
               size={"xl"}
               onOpenChange={onLoginOpenChange}
               placement={"center"}
               isOpen={isLoginOpen}
            >
               <ModalContent>
                  {(onClose) => (
                     <>
                        <ModalHeader className={`flex flex-col gap-1`}>
                           Log in
                        </ModalHeader>
                        <ModalBody>
                           <Input
                              autoFocus
                              endContent={
                                 null
                                 // <MailIcon className="text-2xl text-default-400 pointer-events-none flex-shrink-0" />
                              }
                              label="Email"
                              placeholder="Enter your email"
                              variant="bordered"
                           />
                           <Input
                              endContent={
                                 null
                                 // <LockIcon className="text-2xl text-default-400 pointer-events-none flex-shrink-0" />
                              }
                              label="Password"
                              placeholder="Enter your password"
                              type="password"
                              variant="bordered"
                           />
                           <div className="flex py-2 px-1 justify-between">
                              <Checkbox
                                 classNames={{
                                    label: "text-small",
                                 }}
                              >
                                 Remember me
                              </Checkbox>
                              <Link color="primary" href="#" size="sm">
                                 Forgot password?
                              </Link>
                           </div>
                        </ModalBody>
                        <ModalFooter>
                           <Button
                              color="danger"
                              variant="shadow"
                              onClick={onClose}
                           >
                              Close
                           </Button>
                           <Button color="primary" onPress={onClose}>
                              Action
                           </Button>
                        </ModalFooter>
                     </>
                  )}
               </ModalContent>
            </Modal>
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

export default ModalsPage;
