import { VBox } from "@etsoo/materialui";
import { Button, CircularProgress, TextField, Typography } from "@mui/material";
import React from "react";
import { useNavigate, useParams } from "react-router-dom";
import { app } from "../app/SmartApp";
import { SharedLayout } from "./SharedLayout";
import { MemberInvitationDto } from "@etsoo/smarterp-core";
import { useSearchParamsEx } from "@etsoo/react";
import { Constants } from "../app/Constants";

export default function Invite() {
  // Router
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const { code } = useSearchParamsEx({ code: "string" });

  // Labels
  const labels = app.getLabels(
    "acceptInvitation",
    "email",
    "inviteMemberDone",
    "inviterOrg",
    "inviter",
    "inviteTipMessage",
    "loading",
    "ok"
  );

  // State
  const [data, setData] = React.useState<MemberInvitationDto>();
  const [subTitle, setSubTitle] = React.useState<string>();

  // Mount
  const isMounted = React.useRef(true);

  // Is loading
  const isLoading = data == null;

  // Button label
  const buttonLabel = isLoading ? labels.loading : labels.ok;

  // Email
  const email =
    id && data?.email ? app.decrypt(data.email, id.substring(0, 4)) : undefined;

  // Button click handler
  const buttonHandler =
    data == null || isLoading
      ? undefined
      : () => {
          app.storage.setData(Constants.MemberInvitation, [id, code]);
          if (email) {
            if (data.userExists) {
              navigate(
                `./../../login/password/${encodeURIComponent(
                  app.encrypt(email)
                )}`
              );
            } else {
              navigate(
                `./../../login/register10?openid=${encodeURIComponent(email)}`
              );
            }
          } else {
            navigate("./../../");
          }
        };

  // Ready
  React.useEffect(() => {
    if (!id || !code) return;

    // Labels
    const { inviteMemberExpired, inviteMemberDone } = app.getLabels(
      "inviteMemberExpired",
      "inviteMemberDone"
    );

    // Query data
    app.publicApi.readInvitation(id).then((result) => {
      // Unmounted
      if (!isMounted.current) return;

      if (result == null) {
        setSubTitle(inviteMemberDone);
        return;
      }

      // Update data
      setData(result);

      // Validate
      if (result.isExpired) {
        setSubTitle(inviteMemberExpired);
        return;
      }
    });
  }, [id]);

  React.useEffect(() => {
    return () => {
      isMounted.current = false;
    };
  }, []);

  return (
    <SharedLayout
      title={labels.acceptInvitation}
      subTitle={subTitle}
      buttons={[
        data?.isAccepted ? (
          <Button variant="contained" onClick={() => navigate("./../../")}>
            {labels.inviteMemberDone}
          </Button>
        ) : (
          <></>
        ),
        <Button
          key="submit"
          onClick={buttonHandler}
          variant="outlined"
          disabled={isLoading || data.isAccepted}
          endIcon={isLoading ? <CircularProgress size={12} /> : undefined}
        >
          {buttonLabel}
        </Button>
      ]}
    >
      {data && (
        <VBox gap={1} width="100%">
          <TextField
            margin="dense"
            variant="standard"
            label={labels.email}
            value={email?.hideEmail()}
            disabled
          />
          <TextField
            margin="dense"
            variant="standard"
            label={labels.inviter}
            value={data.inviter}
            disabled
          />
          <TextField
            margin="dense"
            variant="standard"
            label={labels.inviterOrg}
            value={data.orgName}
            disabled
          />
          <Typography variant="caption">{labels.inviteTipMessage}</Typography>
        </VBox>
      )}
    </SharedLayout>
  );
}
