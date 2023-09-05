"use client";
import {
   Button,
   Link,
   Modal,
   ModalBody,
   ModalContent,
   ModalFooter,
   ModalHeader,
   Spinner,
   useDisclosure,
} from "@nextui-org/react";
import React from "react";
import CrossIcon from "@components/icons/CrossIcon";
import {
   useAcceptCookiePolicyMutation,
   useDeclineCookiePolicyMutation,
} from "@web/api";

export interface CookieConsentBannerProps {
   isOpen: boolean;
   onOpenChange: () => void;
}

export const CookieConsentBannerWrapper = () => {
   const { isOpen, onOpenChange } = useDisclosure({ defaultOpen: true });
   return <CookieConsentBanner isOpen={isOpen} onOpenChange={onOpenChange} />;
};

const CookieConsentBanner = ({
   isOpen,
   onOpenChange,
}: CookieConsentBannerProps) => {
   const { mutateAsync: acceptCookiePolicy, isLoading: acceptLoading } =
      useAcceptCookiePolicyMutation();
   const { mutateAsync: declineCookiePolicy, isLoading: declineLoading } =
      useDeclineCookiePolicyMutation();

   const handleAcceptCookiePolicy = async () => {
      await acceptCookiePolicy({});
   };

   const handleDeclineCookiePolicy = async () => {
      await declineCookiePolicy({});
   };

   return (
      <Modal
         backdrop={"transparent"}
         classNames={{
            base: "absolute text-xs sm:my-0 my-0 right-auto bg-default-100 opacity-90 bottom-10 w-fit min-w-fit max-w-fit mx-auto",
         }}
         shadow={"md"}
         closeButton={
            <Button
               size={"sm"}
               radius={"full"}
               variant={"light"}
               isIconOnly
               className={`top-2 p-0 right-2`}
               color={"default"}
            >
               <CrossIcon
                  className={`fill-transparent stroke-foreground`}
                  size={12}
               />
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
         isOpen={isOpen}
         onOpenChange={onOpenChange}
      >
         <ModalContent>
            <ModalHeader className={`pb-2 text-small`}>
               🍪 Cookie Consent Notice 🍪
            </ModalHeader>
            <ModalBody className={`pb-2 pt-0`}>
               <p>
                  We use cookies to enhance your experience on our chat
                  application. By clicking{" "}
                  <strong className={`inline`}> "Accept" </strong>, you agree to
                  our use of cookies <br /> as described in our{" "}
                  <Link
                     className={`text-xs`}
                     size={"sm"}
                     underline={"hover"}
                     color={"primary"}
                     href={`/terms-of-service`}
                  >
                     Terms of Service.
                  </Link>
               </p>
            </ModalBody>
            <ModalFooter
               className={`pt-0 flex justify-start text-xs items-center gap-6`}
            >
               <Button
                  onPress={async (_) => {
                     await handleAcceptCookiePolicy();
                     onOpenChange();
                  }}
                  isLoading={acceptLoading}
                  spinner={<Spinner color={"white"} size={"sm"} />}
                  size={"sm"}
                  className={`text-xs`}
                  color={"success"}
                  variant={"light"}
               >
                  {!acceptLoading && "Accept and Continue"}
               </Button>
               <Link
                  underline={`hover`}
                  className={`text-xs`}
                  href={`/cookie-policy`}
                  size={"sm"}
                  color={"primary"}
               >
                  Learn more
               </Link>
               <Button
                  onPress={async (_) => {
                     await handleDeclineCookiePolicy();
                     onOpenChange();
                  }}
                  isLoading={declineLoading}
                  size={"sm"}
                  spinner={<Spinner color={"white"} size={"sm"} />}
                  className={`px-6 text-xs py-1`}
                  color={"danger"}
               >
                  {!declineLoading && "Decline"}
               </Button>
            </ModalFooter>
         </ModalContent>
      </Modal>
   );
};

export default CookieConsentBanner;
