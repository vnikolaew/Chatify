import {
   Avatar,
   Button,
   Card,
   CardBody,
   CardFooter,
   CardHeader,
   Modal,
   ModalBody,
   ModalContent,
   ModalFooter,
   ModalHeader,
   Skeleton,
   Spinner,
   Tooltip,
   useDisclosure,
} from "@nextui-org/react";
import TooltipWithPopoverActionButton from "@components/common/TooltipWithPopoverActionButton";
import { PinIcon } from "@icons";
import React, { Fragment, useMemo, useState } from "react";
import { useCurrentChatGroup } from "@hooks";
import {
   getMediaUrl,
   useGetChatGroupDetailsQuery,
   useGetChatGroupPinnedMessages,
   useUnpinGroupChatMessage,
} from "@web/api";
import { User as TUser } from "@openapi";
import { X } from "lucide-react";
import moment from "moment/moment";

export const PinnedMessagesActionButton = ({}: {}) => {
   const { isOpen, onOpenChange, onOpen } = useDisclosure({
      defaultOpen: false,
   });

   return (
      <TooltipWithPopoverActionButton
         popoverProps={{
            showArrow: true,
            className: `bg-zinc-900 z-20`,
         }}
         isOpen={isOpen}
         onOpenChange={onOpenChange}
         popoverContent={<PinnedMessagesPopover onOpen={onOpen} />}
         tooltipContent={"Pinned messages"}
         icon={<PinIcon fill={"white"} size={20} />}
      />
   );
};

export const PinnedMessagesPopover = ({
   onOpen: keepPopoverOpen,
}: {
   onOpen: () => void;
}) => {
   const groupId = useCurrentChatGroup();
   const { data: groupDetails } = useGetChatGroupDetailsQuery(groupId);
   const {
      data: pinnedMessages,
      isLoading,
      isFetching,
      error,
   } = useGetChatGroupPinnedMessages({ groupId });
   const [isOpen, setOpen] = useState(false);

   const {
      mutateAsync: unpinMessage,
      error: unpinError,
      isLoading: unpinLoading,
   } = useUnpinGroupChatMessage();

   const pinnedMessagesSenders = useMemo<Record<string, TUser>>(() => {
      return groupDetails?.members
         .filter((m) => pinnedMessages?.some((pm) => pm.userId === m.id))
         .reduce((acc, m) => {
            acc[m.id] = m;
            return acc;
         }, {} as Record<string, TUser>);
   }, [pinnedMessages, groupDetails?.members]);

   const handleUnpinMessage = async (messageId: string) => {
      await unpinMessage({ groupId, messageId });
   };

   return isLoading && isFetching ? (
      <div className={`flex my-2 flex-col items-start gap-4`}>
         {Array.from({ length: 3 }).map((_, i) => (
            <div className={`flex flex-col items-start gap-1`} key={i}>
               <div className={`flex items-center gap-2`}>
                  <Skeleton className={`w-8 h-8 rounded-full`} />
                  <Skeleton className={`w-20 h-3 rounded-full`} />
               </div>
               <div className={`flex flex-col gap-2 w-full`}>
                  <Skeleton className={`w-48 h-4 rounded-full`} />
                  <Skeleton className={`w-12 h-1 rounded-full`} />
               </div>
            </div>
         ))}
      </div>
   ) : (
      <div className={`flex my-2 flex-col items-start gap-2`}>
         {pinnedMessages && !pinnedMessages.length && (
            <div className={`text-xs text-default-400 my-4 mx-2`}>
               There are no pinned messages in this group.
            </div>
         )}
         {pinnedMessages?.map((m) => (
            <Card
               className={`max-w-[400px] p-2 relative border-1 border-default-300`}
               isBlurred
               shadow={"md"}
               radius={"md"}
               key={m.id}
            >
               <Modal
                  placement={`center`}
                  radius={`md`}
                  size={`md`}
                  draggable
                  shadow={`sm`}
                  className={`z-200000`}
                  backdrop={`opaque`}
                  isOpen={isOpen}
                  onOpenChange={setOpen}
               >
                  <ModalContent className={``}>
                     {(onClose) => (
                        <Fragment>
                           <ModalHeader className={`text-medium`}>
                              Remove pinned message
                           </ModalHeader>
                           <ModalBody className={`text-small`}>
                              Are you sure you want to remove this pinned
                              message?
                           </ModalBody>
                           <ModalFooter className={`text-xs`}>
                              <Button
                                 onPress={onClose}
                                 size={`sm`}
                                 variant={`bordered`}
                                 color={`default`}
                              >
                                 Cancel
                              </Button>
                              <Button
                                 size={`sm`}
                                 variant={`shadow`}
                                 className={`text-foreground`}
                                 isLoading={unpinLoading}
                                 isDisabled={unpinLoading}
                                 spinner={
                                    <Spinner color={`white`} size={`sm`} />
                                 }
                                 color={`success`}
                                 onPress={async (e) => {
                                    await handleUnpinMessage(m.id);
                                    e.continuePropagation();
                                    onClose();
                                 }}
                              >
                                 {unpinLoading
                                    ? `Loading ...`
                                    : `Remove message`}
                              </Button>
                           </ModalFooter>
                        </Fragment>
                     )}
                  </ModalContent>
               </Modal>
               <CardHeader
                  className={`flex relative items-center justify-start py-2 gap-2`}
               >
                  <div className={`absolute inline z-20 top-2 right-0`}>
                     <Tooltip
                        content={`Un-pin this message`}
                        delay={100}
                        closeDelay={100}
                        radius={`sm`}
                        size={`sm`}
                        shadow={`sm`}
                        classNames={{
                           base: `text-[.6rem]`,
                        }}
                        color={`default`}
                     >
                        <Button
                           className={`w-6 h-6 p-1 bg-transparent`}
                           radius={`full`}
                           onPress={(e) => {
                              setOpen(true);
                              // keepPopoverOpen();
                           }}
                           startContent={
                              <X
                                 size={12}
                                 className={`stroke-default-400 transition-all duration-100 group-hover:stroke-foreground`}
                              />
                           }
                           isIconOnly
                        />
                     </Tooltip>
                  </div>
                  <Avatar
                     src={getMediaUrl(
                        pinnedMessagesSenders[m.userId]?.profilePicture.mediaUrl
                     )}
                     size={"sm"}
                     radius={`full`}
                  />
                  <span>{pinnedMessagesSenders[m.userId]?.username}</span>
               </CardHeader>
               <CardBody className={`py-1 px-2 text-xs`}>{m.content}</CardBody>
               <CardFooter
                  className={`text-[.6rem] text-default-400 mt-2 p-0 px-2`}
               >
                  {moment(new Date(m.createdAt)).format(`MMM HH:MM A`)}
               </CardFooter>
            </Card>
         ))}
      </div>
   );
};
