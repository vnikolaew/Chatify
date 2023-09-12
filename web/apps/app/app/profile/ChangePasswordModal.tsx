"use client";
import {
   Button,
   Modal,
   ModalBody,
   ModalContent,
   ModalFooter,
   ModalHeader,
   ModalProps,
   Spinner,
} from "@nextui-org/react";
import React, { Fragment, useState } from "react";
import PasswordInput from "../signin/PasswordInput";
import { usePassword } from "@hooks";
import { sleep, useChangeUserPasswordMutation } from "@web/api";
import Toast from "@components/common/Toast";

export interface ChangePasswordModalProps
   extends Omit<ModalProps, "children"> {}

const ChangePasswordModal = (props: ChangePasswordModalProps) => {
   const {
      validationState: passwordState,
      errorMessage: passwordErrorMessage,
      password: newPassword,
      setPassword: setNewPassword,
   } = usePassword();
   const {
      mutateAsync: changePassword,
      error: changeError,
      isLoading: changeLoading,
   } = useChangeUserPasswordMutation();

   const [oldPassword, setOldPassword] = useState(``);
   const [passwordChangeSuccess, setPasswordChangeSuccess] = useState(false);

   async function handleChangePassword() {
      await changePassword(
         {
            oldPassword,
            newPassword,
         },
         { onSuccess: (_) => setPasswordChangeSuccess(true) }
      );
   }

   // @ts-ignore
   return (
      <Fragment>
         {passwordChangeSuccess && (
            <Toast
               isOpen={true}
               onOpenChange={setPasswordChangeSuccess}
               header={"Success!"}
               size={`md`}
               className={`min-w-[300px] bg-success-300`}
               shadow={`md`}
            >
               Password successfully changed.
            </Toast>
         )}
         <Modal
            size={`lg`}
            radius={`md`}
            backdrop={`opaque`}
            shadow={`md`}
            placement={`center`}
            {...props}
         >
            <ModalContent className={`p-2 py-2`}>
               {(onClose) => (
                  <Fragment>
                     <ModalHeader className={`text-lg`}>
                        Change your password
                     </ModalHeader>
                     <ModalBody>
                        <PasswordInput
                           onValueChange={setOldPassword}
                           value={oldPassword}
                           className={`w-full`}
                           labelPlacement={`outside`}
                           placeholder={"•••••"}
                           label={`Old password`}
                        />
                        <PasswordInput
                           value={newPassword}
                           // @ts-ignore
                           validationState={passwordState}
                           errorMessage={passwordErrorMessage}
                           onValueChange={setNewPassword}
                           labelPlacement={`outside`}
                           description={`We will not share your credentials with any third party under any circumstances.`}
                           placeholder={"•••••"}
                           className={`w-full mt-4`}
                           classNames={{
                              description: `mt-1`,
                           }}
                           label={`New password`}
                        />
                     </ModalBody>
                     <ModalFooter className={`mt-8`}>
                        <Button
                           variant={`solid`}
                           onPress={onClose}
                           className={`mr-2 w-1/4`}
                           size={`sm`}
                           color={`danger`}
                        >
                           Cancel
                        </Button>
                        <Button
                           variant={`shadow`}
                           size={`sm`}
                           isLoading={changeLoading}
                           isDisabled={changeLoading}
                           spinner={<Spinner color={`white`} size={`sm`} />}
                           onPress={async (_) => {
                              await handleChangePassword();
                              onClose();
                           }}
                           className={`w-1/3`}
                           color={`primary`}
                        >
                           {changeLoading ? "Loading" : "Update"}
                        </Button>
                     </ModalFooter>
                  </Fragment>
               )}
            </ModalContent>
         </Modal>
      </Fragment>
   );
};

export default ChangePasswordModal;
