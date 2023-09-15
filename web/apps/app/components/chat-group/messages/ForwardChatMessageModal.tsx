"use client";
import React, {
   Fragment,
   useCallback,
   useEffect,
   useMemo,
   useState,
} from "react";
import {
   Avatar,
   Button,
   Chip,
   Divider,
   Input,
   Modal,
   ModalBody,
   ModalContent,
   ModalFooter,
   ModalHeader,
   Spinner,
} from "@nextui-org/react";
import {
   getMediaUrl,
   useForwardChatMessage,
   useGetUserDetailsQuery,
   useSearchChatGroupsByName,
} from "@web/api";
import { useClickAway } from "@uidotdev/usehooks";
import { ChatGroup, ChatMessage } from "@openapi";
import { useDebounce } from "@hooks";
import ChatGroupSearchEntries from "@components/chat-group/messages/ChatGroupSearchEntries";
import { X } from "lucide-react";

export interface ForwardChatMessageModalProps {
   isOpen: boolean;
   onOpenChange: () => void;
   message: ChatMessage;
}

const ForwardChatMessageModal = ({
   onOpenChange,
   message,
   isOpen,
}: ForwardChatMessageModalProps) => {
   const {
      mutateAsync: forwardMessage,
      isLoading,
      error,
   } = useForwardChatMessage();
   const [groupSearchQuery, setGroupSearchQuery] = useState(``);
   const debouncedSearch = useDebounce(groupSearchQuery, 2000);
   const {
      data: searchEntries,
      isLoading: searchLoading,
      isFetching: searchFetching,
      refetch,
      error: searchError,
   } = useSearchChatGroupsByName(
      { query: debouncedSearch },
      { enabled: debouncedSearch?.length >= 2 }
   );
   const { data: sender } = useGetUserDetailsQuery(message.userId);
   const [selectedGroups, setSelectedGroups] = useState<ChatGroup[]>([]);
   const [isInputFocused, setIsInputFocused] = useState(false);
   const [isDatalistOpen, setIsDatalistOpen] = useState(false);
   const inputRef = useClickAway<HTMLInputElement>(() =>
      setIsDatalistOpen(false)
   );
   const focusInput = useCallback(
      () => inputRef.current?.focus(),
      [inputRef?.current]
   );

   const isDropdownOpen = useMemo(
      () => (searchEntries?.length > 0 ?? false) && isDatalistOpen,
      [isDatalistOpen, searchEntries?.length]
   );

   // useEffect(() => focusInput(), []);
   useEffect(() => {
      if (isInputFocused) setIsDatalistOpen(true);
   }, [isInputFocused]);

   return (
      <Modal
         shadow={"md"}
         radius={"sm"}
         placement={`center`}
         size={`lg`}
         onOpenChange={onOpenChange}
         isOpen={isOpen}
      >
         <ModalContent className={`overflow-visible`}>
            {(onClose) => (
               <Fragment>
                  <ModalHeader>Forward this message</ModalHeader>
                  <ModalBody>
                     <div className={`relative`}>
                        <Input
                           color={`default`}
                           value={groupSearchQuery}
                           isClearable
                           ref={inputRef}
                           onFocusChange={setIsInputFocused}
                           onFocus={async (_) => {
                              if (!searchEntries?.length) {
                                 await refetch({});
                              }
                           }}
                           startContent={
                              !!selectedGroups.length ? (
                                 <div className={`flex items-center gap-1`}>
                                    {selectedGroups.map((group: ChatGroup) => (
                                       <Chip
                                          size={`sm`}
                                          classNames={{
                                             content: `px-2 font-normal text-xs`,
                                          }}
                                          startContent={
                                             <Avatar
                                                src={group?.picture?.mediaUrl}
                                                className={`w-4 h-4`}
                                             />
                                          }
                                          endContent={
                                             <X
                                                onClick={(_) => {
                                                   setSelectedGroups((g) =>
                                                      g.filter(
                                                         (g) =>
                                                            g.id !== group.id
                                                      )
                                                   );
                                                   focusInput();
                                                }}
                                                className={`stroke-default-400 mr-1 transition-all duration-100 hover:stroke-default-600 cursor-pointer`}
                                                size={12}
                                             />
                                          }
                                          key={group.id}
                                       >
                                          {group.name}
                                       </Chip>
                                    ))}
                                 </div>
                              ) : (
                                 null!
                              )
                           }
                           onValueChange={setGroupSearchQuery}
                           variant={`bordered`}
                           size={`md`}
                           radius={`sm`}
                           className={`w-full`}
                           classNames={{
                              input: `pl-2`,
                           }}
                           placeholder={`Search for a group or a person`}
                           type={`text`}
                        />
                        {isDropdownOpen && (
                           <ChatGroupSearchEntries
                              loading={searchLoading && searchFetching}
                              setSelectedEntries={setSelectedGroups}
                              selectedEntries={selectedGroups}
                              onSelect={(group) => {
                                 setSelectedGroups((g) => [...g, group]);
                                 focusInput();
                              }}
                              entries={searchEntries.filter(
                                 (e) => e.id !== message.chatGroupId
                              )}
                           />
                        )}
                     </div>
                     <div
                        className={`flex mt-2 items-center justify-start w-full gap-4`}
                     >
                        <Divider
                           className={`h-20 w-[2px] rounded-full bg-foreground-300`}
                           orientation={`vertical`}
                        />
                        <div className={`flex flex-col items-start gap-2`}>
                           <div className={`flex items-center gap-4`}>
                              <Avatar
                                 className={`w-6 h-6`}
                                 radius={`sm`}
                                 size={`sm`}
                                 src={getMediaUrl(
                                    sender?.user?.profilePicture?.mediaUrl
                                 )}
                              />
                              <span
                                 className={`text-small text-foreground-600`}
                              >
                                 {sender?.user?.username}
                              </span>
                           </div>
                           <p
                              dangerouslySetInnerHTML={{
                                 __html: message.content,
                              }}
                              className={`text-xs w-2/3 text-default-400`}
                           ></p>
                        </div>
                     </div>
                  </ModalBody>
                  <ModalFooter className={`mt-4`}>
                     <Button
                        variant={`bordered`}
                        radius={`sm`}
                        size={`md`}
                        color={`default`}
                        onPress={onClose}
                     >
                        Save Draft
                     </Button>
                     <Button
                        className={`ml-2`}
                        isLoading={isLoading}
                        isDisabled={isLoading}
                        spinner={<Spinner color={`white`} size={`sm`} />}
                        variant={`solid`}
                        radius={`sm`}
                        size={`md`}
                        color={`success`}
                        onPress={async (_) => {
                           await forwardMessage({
                              messageId: message.id,
                              groupId: message.chatGroupId,
                              groupIds: selectedGroups.map((_) => _.id),
                              content: ``,
                           });
                           console.log(selectedGroups);
                           onClose();
                        }}
                     >
                        {isLoading ? "Loading ..." : `Forward`}
                     </Button>
                  </ModalFooter>
               </Fragment>
            )}
         </ModalContent>
      </Modal>
   );
};

export default ForwardChatMessageModal;
