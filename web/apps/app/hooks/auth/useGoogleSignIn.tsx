import { useGoogleLogin } from "@react-oauth/google";
import { useGoogleSignUpMutation } from "@web/api";

export function useGoogleSignIn(
   onSuccess?: (response: any) => void | Promise<void>
) {
   const {
      error,
      data,
      isLoading,
      mutateAsync: googleSignUp,
   } = useGoogleSignUpMutation();

   const login = useGoogleLogin({
      onSuccess: ({ access_token, scope }) => {
         googleSignUp(
            { accessToken: access_token },
            {
               onError: (err) =>
                  console.error("A sign up error occurred: ", err),
            }
         )
            .then((r) => {
               console.log("Google login data: ", data);
               onSuccess?.(r);
            })
            .catch((err) => {
               console.log("Google login error: ", error);
            });
      },
      scope: "https://www.googleapis.com/auth/userinfo.profile",
      flow: "implicit",
      onError: console.error,
   });

   return { login, isLoading, error, data };
}
